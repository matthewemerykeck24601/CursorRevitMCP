import Foundation

/// Direct xAI Responses API (Grok) — mirrors `monty-ai-server` / `aps-ai-web` behavior.
enum XaiChatClientError: LocalizedError {
    case missingApiKey
    case http(Int, String)

    var errorDescription: String? {
        switch self {
        case .missingApiKey:
            return "Add your xAI API key in Settings."
        case .http(let code, let body):
            return "xAI error \(code): \(body)"
        }
    }
}

enum XaiChatClient {
    private static let endpoint = URL(string: "https://api.x.ai/v1/responses")!

    private static let instructions = [
        "You are Grok, built by xAI, helping the user in the Monty iOS app (Autodesk ACC / construction when relevant).",
        "Be direct, helpful, and conversational — witty when it fits. Skip corporate filler.",
        "Use markdown when it helps. If you lack ACC-specific data, say so instead of inventing it.",
    ].joined(separator: "\n")

    static func send(
        apiKey: String,
        model: String,
        userMessage: String,
        chatHistory: [String],
        selectedHubId: String?,
    ) async throws -> String {
        let key = apiKey.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !key.isEmpty else { throw XaiChatClientError.missingApiKey }

        var chunks: [String] = []
        if !chatHistory.isEmpty {
            chunks.append("Conversation so far:\n" + chatHistory.joined(separator: "\n"))
        }
        if let hub = selectedHubId, !hub.isEmpty {
            chunks.append("Context: selected Autodesk hub id = \(hub)")
        }
        chunks.append("User message:\n\(userMessage)")

        let body: [String: Any] = [
            "model": model,
            "temperature": 0.65,
            "store": false,
            "max_output_tokens": 8192,
            "instructions": instructions,
            "input": chunks.joined(separator: "\n\n"),
        ]
        let data = try JSONSerialization.data(withJSONObject: body)

        var req = URLRequest(url: endpoint)
        req.httpMethod = "POST"
        req.setValue("Bearer \(key)", forHTTPHeaderField: "Authorization")
        req.setValue("application/json", forHTTPHeaderField: "Content-Type")
        req.httpBody = data

        let (respData, response) = try await URLSession.shared.data(for: req)
        guard let http = response as? HTTPURLResponse else {
            throw XaiChatClientError.http(-1, "No HTTP response")
        }
        let text = String(data: respData, encoding: .utf8) ?? ""
        guard http.statusCode == 200 else {
            throw XaiChatClientError.http(http.statusCode, text)
        }

        let out = extractOutputText(from: respData)
        return out.isEmpty ? "(no text in model response)" : out
    }

    /// Walks xAI `responses` JSON (handles minor schema variations).
    private static func extractOutputText(from data: Data) -> String {
        guard let obj = try? JSONSerialization.jsonObject(with: data) as? [String: Any] else {
            return ""
        }
        var parts: [String] = []
        if let output = obj["output"] as? [[String: Any]] {
            for item in output {
                guard let content = item["content"] as? [[String: Any]] else { continue }
                for c in content {
                    if (c["type"] as? String) == "output_text", let t = c["text"] as? String {
                        parts.append(t)
                    }
                }
            }
        }
        return parts.joined(separator: "\n").trimmingCharacters(in: .whitespacesAndNewlines)
    }
}
