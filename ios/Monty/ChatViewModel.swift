import Foundation

struct ChatMessage: Identifiable, Equatable {
    enum Role {
        case user
        case assistant
        case system
    }

    let id = UUID()
    let role: Role
    let text: String
}

@MainActor
final class ChatViewModel: ObservableObject {
    @Published var messages: [ChatMessage] = []
    @Published var input: String = ""
    @Published var isSending = false
    @Published var lastError: String?

    private let api = ChatAPIService()

    init() {
        messages.append(
            ChatMessage(
                role: .system,
                text:
                    "Monty — admin task mode. Describe what to do (for example: add user@company.com to project JOB12345). Hub context is sent with each message.",
            ),
        )
    }

    func send(baseURL: URL?, selectedHubId: String?) async {
        let trimmed = input.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !trimmed.isEmpty else { return }
        guard let baseURL else {
            lastError = "Set the server URL in Settings."
            return
        }

        lastError = nil
        let priorHistory = buildChatHistory()
        messages.append(ChatMessage(role: .user, text: trimmed))
        input = ""
        isSending = true
        defer { isSending = false }

        let history = priorHistory

        do {
            let parsed = try await api.sendAdminTask(
                baseURL: baseURL,
                userMessage: trimmed,
                chatHistory: history,
                selectedHubId: selectedHubId,
            )
            var reply = parsed.message
            if reply.isEmpty {
                reply = "(Empty reply)"
            }
            if let extra = parsed.queryResultSummary, !extra.isEmpty {
                reply += "\n\n— queryResult —\n\(extra)"
            }
            if let rid = parsed.requestId {
                reply += "\n\nrequestId: \(rid)"
            }
            messages.append(ChatMessage(role: .assistant, text: reply))
        } catch let error as ChatAPIError {
            if case .notAuthenticated = error {
                NotificationCenter.default.post(name: .montySessionExpired, object: nil)
            }
            lastError = error.localizedDescription
            messages.append(
                ChatMessage(
                    role: .assistant,
                    text: "Error: \(error.localizedDescription)",
                ),
            )
        } catch {
            lastError = error.localizedDescription
            messages.append(
                ChatMessage(
                    role: .assistant,
                    text: "Error: \(error.localizedDescription)",
                ),
            )
        }
    }

    private func buildChatHistory() -> [String] {
        messages
            .filter { $0.role != .system }
            .suffix(12)
            .map { m in
                switch m.role {
                case .user: return "user: \(m.text)"
                case .assistant: return "assistant: \(m.text)"
                case .system: return m.text
                }
            }
    }
}
