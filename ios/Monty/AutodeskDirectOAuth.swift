import Foundation

struct AutodeskTokenResponse: Decodable {
    let access_token: String
    let token_type: String?
    let expires_in: Int
    let refresh_token: String?
    let scope: String?
}

enum AutodeskDirectOAuthError: LocalizedError {
    case http(Int, String)
    case invalidResponse

    var errorDescription: String? {
        switch self {
        case .http(let c, let b):
            return "Autodesk token (\(c)): \(b)"
        case .invalidResponse:
            return "Invalid token response."
        }
    }
}

/// PKCE code exchange and refresh — **on device**, no `aps-ai-web` (public client + PKCE).
enum AutodeskDirectOAuth {
    static func exchangeCodeForTokens(
        code: String,
        codeVerifier: String,
        redirectUri: String,
        clientId: String,
    ) async throws -> AutodeskTokenResponse {
        var body = URLComponents()
        body.queryItems = [
            URLQueryItem(name: "grant_type", value: "authorization_code"),
            URLQueryItem(name: "code", value: code),
            URLQueryItem(name: "redirect_uri", value: redirectUri),
            URLQueryItem(name: "client_id", value: clientId),
            URLQueryItem(name: "code_verifier", value: codeVerifier),
        ]
        let encoded = body.percentEncodedQuery ?? ""
        return try await postToken(body: encoded)
    }

    static func refreshTokens(
        refreshToken: String,
        clientId: String,
        scope: String,
    ) async throws -> AutodeskTokenResponse {
        var body = URLComponents()
        body.queryItems = [
            URLQueryItem(name: "grant_type", value: "refresh_token"),
            URLQueryItem(name: "refresh_token", value: refreshToken),
            URLQueryItem(name: "client_id", value: clientId),
            URLQueryItem(name: "scope", value: scope),
        ]
        let encoded = body.percentEncodedQuery ?? ""
        return try await postToken(body: encoded)
    }

    private static func postToken(body: String) async throws -> AutodeskTokenResponse {
        guard let url = URL(string: AutodeskOAuthConfig.tokenEndpoint) else {
            throw AutodeskDirectOAuthError.invalidResponse
        }
        var request = URLRequest(url: url)
        request.httpMethod = "POST"
        request.setValue("application/x-www-form-urlencoded", forHTTPHeaderField: "Content-Type")
        request.httpBody = body.data(using: .utf8)

        let (data, response) = try await URLSession.shared.data(for: request)
        guard let http = response as? HTTPURLResponse else {
            throw AutodeskDirectOAuthError.invalidResponse
        }
        let text = String(data: data, encoding: .utf8) ?? ""
        guard http.statusCode == 200 else {
            throw AutodeskDirectOAuthError.http(http.statusCode, text)
        }
        return try JSONDecoder().decode(AutodeskTokenResponse.self, from: data)
    }
}
