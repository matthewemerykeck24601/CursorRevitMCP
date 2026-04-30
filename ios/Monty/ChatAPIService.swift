import Foundation

struct ChatAPIParsed {
    let message: String
    let requestId: String?
    /// Pretty-printed JSON for admin tool payloads when present.
    let queryResultSummary: String?
}

enum ChatAPIError: LocalizedError {
    case invalidBaseURL
    case notAuthenticated
    case httpStatus(Int, String)
    case invalidJSON

    var errorDescription: String? {
        switch self {
        case .invalidBaseURL:
            return "Set a valid server URL in Settings."
        case .notAuthenticated:
            return "Sign in again, or set Backend URL if using a remote server."
        case .httpStatus(let code, let body):
            return "Server error \(code): \(body)"
        case .invalidJSON:
            return "Could not read JSON response."
        }
    }
}

final class ChatAPIService {
    private let session: URLSession

    init() {
        let config = URLSessionConfiguration.default
        config.httpCookieStorage = .shared
        config.httpCookieAcceptPolicy = .always
        session = URLSession(configuration: config)
    }

    func sendAdminTask(
        baseURL: URL,
        userMessage: String,
        chatHistory: [String],
        selectedHubId: String?,
    ) async throws -> ChatAPIParsed {
        guard let url = URL(string: "api/chat", relativeTo: baseURL)?.absoluteURL else {
            throw ChatAPIError.invalidBaseURL
        }

        var body: [String: Any] = [
            "message": userMessage,
            "workspaceMode": "admin",
            "chatHistory": chatHistory,
        ]
        if let hub = selectedHubId, !hub.isEmpty {
            body["selectedHubId"] = hub
        }
        let data = try JSONSerialization.data(withJSONObject: body)

        var request = URLRequest(url: url)
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        request.httpBody = data
        if let t = KeychainHelper.load(account: MontyTokenAccounts.access), !t.isEmpty {
            request.setValue("Bearer \(t)", forHTTPHeaderField: "Authorization")
        }

        let (respData, response) = try await session.data(for: request)
        guard let http = response as? HTTPURLResponse else {
            throw ChatAPIError.httpStatus(-1, "No HTTP response")
        }
        let text = String(data: respData, encoding: .utf8) ?? ""

        switch http.statusCode {
        case 200:
            break
        case 401:
            throw ChatAPIError.notAuthenticated
        default:
            throw ChatAPIError.httpStatus(http.statusCode, text)
        }

        guard let obj = try JSONSerialization.jsonObject(with: respData) as? [String: Any] else {
            throw ChatAPIError.invalidJSON
        }
        let message = (obj["message"] as? String)?.trimmingCharacters(in: .whitespacesAndNewlines) ?? ""
        let requestId = obj["requestId"] as? String
        var summary: String?
        if let qr = obj["queryResult"], JSONSerialization.isValidJSONObject(qr),
           let d = try? JSONSerialization.data(withJSONObject: qr, options: [.prettyPrinted, .sortedKeys]),
           let s = String(data: d, encoding: .utf8) {
            summary = s
        }
        return ChatAPIParsed(message: message, requestId: requestId, queryResultSummary: summary)
    }
}
