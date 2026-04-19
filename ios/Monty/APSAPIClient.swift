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

private struct HubsAPIResponse: Decodable {
    let hubs: [HubInfo]
}

private struct SessionAPIResponse: Decodable {
    let authenticated: Bool?
    /// Milliseconds since epoch (APS session expiry).
    let expiresAt: Int64?
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

/// Calls `aps-ai-web` JSON routes with shared cookie session.
final class APSAPIClient {
    private let session: URLSession

    init() {
        let config = URLSessionConfiguration.default
        config.httpCookieStorage = .shared
        config.httpCookieAcceptPolicy = .always
        session = URLSession(configuration: config)
    }

    func fetchSessionStatus(baseURL: URL) async throws -> SessionStatus {
        guard let url = URL(string: "api/auth/session", relativeTo: baseURL)?.absoluteURL else {
            throw APSClientError.invalidURL
        }
        var request = URLRequest(url: url)
        request.httpMethod = "GET"

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

        if let decoded = try? JSONDecoder().decode(SessionAPIResponse.self, from: data) {
            let exp: Date? = decoded.expiresAt.map { Date(timeIntervalSince1970: Double($0) / 1000.0) }
            return .authenticated(expiresAt: exp)
        }
        return .authenticated(expiresAt: nil)
    }

    func fetchHubs(baseURL: URL) async throws -> [HubInfo] {
        guard let url = URL(string: "api/aps/hubs", relativeTo: baseURL)?.absoluteURL else {
            throw APSClientError.invalidURL
        }
        var request = URLRequest(url: url)
        request.httpMethod = "GET"

        let (data, response) = try await session.data(for: request)
        guard let http = response as? HTTPURLResponse else {
            throw APSClientError.http(-1, "No response")
        }
        let text = String(data: data, encoding: .utf8) ?? ""

        if http.statusCode == 401 {
            throw APSClientError.notAuthenticated
        }
        guard http.statusCode == 200 else {
            throw APSClientError.http(http.statusCode, text)
        }

        do {
            let decoded = try JSONDecoder().decode(HubsAPIResponse.self, from: data)
            return decoded.hubs
        } catch {
            throw APSClientError.decoding(error)
        }
    }

    /// Calls `POST /api/admin/add-users-to-projects` (same server logic as chat tool `admin_add_users_to_projects`).
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
