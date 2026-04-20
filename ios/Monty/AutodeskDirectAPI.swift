import Foundation

/// Direct calls to `developer.api.autodesk.com` (hubs, etc.) — no `aps-ai-web` required.
enum AutodeskDirectAPI {
    private static let base = "https://developer.api.autodesk.com"

    struct DMHubsEnvelope: Decodable {
        let data: [DMHub]
    }

    struct DMHub: Decodable {
        let id: String
        let attributes: HubAttributes?
        struct HubAttributes: Decodable {
            let name: String?
        }
    }

    static func fetchHubs(accessToken: String) async throws -> [HubInfo] {
        guard let url = URL(string: "\(base)/project/v1/hubs") else {
            throw APSClientError.invalidURL
        }
        var request = URLRequest(url: url)
        request.httpMethod = "GET"
        request.setValue("Bearer \(accessToken)", forHTTPHeaderField: "Authorization")
        request.setValue("application/json", forHTTPHeaderField: "Accept")

        let (data, response) = try await URLSession.shared.data(for: request)
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

        let decoded = try JSONDecoder().decode(DMHubsEnvelope.self, from: data)
        return decoded.data.map { h in
            HubInfo(
                id: h.id,
                name: h.attributes?.name ?? h.id,
                type: nil,
            )
        }
    }
}
