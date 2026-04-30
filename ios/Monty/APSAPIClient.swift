import Foundation

struct HubInfo: Identifiable, Decodable, Equatable {
    let id: String
    let name: String
    let type: String?

    enum CodingKeys: String, CodingKey {
        case id
        case name
        case type
    }
}

enum APSClientError: LocalizedError {
    case invalidURL
    case notAuthenticated
    case http(Int, String)
    case decoding(Error)

    var errorDescription: String? {
        switch self {
        case .invalidURL:
            return "Invalid server URL."
        case .notAuthenticated:
            return "Not signed in."
        case .http(let code, let body):
            return "Request failed (\(code)): \(body)"
        case .decoding(let e):
            return e.localizedDescription
        }
    }
}

enum SessionStatus: Equatable {
    case authenticated(expiresAt: Date?)
    case unauthenticated
}

/// Calls `aps-ai-web` when you set a **Backend URL**; sends `Authorization: Bearer` from Keychain (standalone auth).
final class APSAPIClient {
    private let session: URLSession

    init() {
        let config = URLSessionConfiguration.default
        config.httpCookieStorage = .shared
        config.httpCookieAcceptPolicy = .always
        session = URLSession(configuration: config)
    }

    private func applyBearer(_ request: inout URLRequest) {
        if let t = KeychainHelper.load(account: MontyTokenAccounts.access), !t.isEmpty {
            request.setValue("Bearer \(t)", forHTTPHeaderField: "Authorization")
        }
    }

    /// Optional: verify backend + bearer against `GET /api/auth/session` when a server URL is configured.
    func fetchSessionStatus(baseURL: URL) async throws -> SessionStatus {
        guard let url = URL(string: "api/auth/session", relativeTo: baseURL)?.absoluteURL else {
            throw APSClientError.invalidURL
        }
        var request = URLRequest(url: url)
        request.httpMethod = "GET"
        applyBearer(&request)

        let (data, response) = try await session.data(for: request)
        guard let http = response as? HTTPURLResponse else {
            throw APSClientError.http(-1, "No response")
        }
        let text = String(data: data, encoding: .utf8) ?? ""

        if http.statusCode == 401 {
            return .unauthenticated
        }
        guard http.statusCode == 200 else {
            throw APSClientError.http(http.statusCode, text)
        }

        struct SessionAPIResponse: Decodable {
            let expiresAt: Int64?
        }
        if let decoded = try? JSONDecoder().decode(SessionAPIResponse.self, from: data) {
            let exp: Date? = decoded.expiresAt.map { Date(timeIntervalSince1970: Double($0) / 1000.0) }
            return .authenticated(expiresAt: exp)
        }
        return .authenticated(expiresAt: nil)
    }

    func addUsersToProjects(
        baseURL: URL,
        hubId: String,
        projectNumbers: [String],
        emails: [String],
        roleNames: [String],
        region: String,
        dryRun: Bool,
    ) async throws -> String {
        guard let url = URL(string: "api/admin/add-users-to-projects", relativeTo: baseURL)?.absoluteURL else {
            throw APSClientError.invalidURL
        }
        var body: [String: Any] = [
            "hubId": hubId,
            "projectNumbers": projectNumbers,
            "emails": emails,
            "dryRun": dryRun,
            "region": region,
        ]
        if !roleNames.isEmpty {
            body["roleNames"] = roleNames
        }
        let data = try JSONSerialization.data(withJSONObject: body)

        var request = URLRequest(url: url)
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        request.httpBody = data
        applyBearer(&request)

        let (respData, response) = try await session.data(for: request)
        guard let http = response as? HTTPURLResponse else {
            throw APSClientError.http(-1, "No response")
        }
        let text = String(data: respData, encoding: .utf8) ?? ""

        if http.statusCode == 401 {
            throw APSClientError.notAuthenticated
        }
        if http.statusCode == 200 {
            if let obj = try? JSONSerialization.jsonObject(with: respData),
               JSONSerialization.isValidJSONObject(obj),
               let pretty = try? JSONSerialization.data(withJSONObject: obj, options: [.prettyPrinted, .sortedKeys]),
               let s = String(data: pretty, encoding: .utf8) {
                return s
            }
            return text.isEmpty ? "{}" : text
        }
        throw APSClientError.http(http.statusCode, text)
    }
}
