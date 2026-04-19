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
}
