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

    private var xaiModel: String {
        UserDefaults.standard.string(forKey: "monty.xaiModel") ?? "grok-3-latest"
    }

    init() {
        messages.append(
            ChatMessage(
                role: .system,
                text:
                    "Monty — chat: paste your xAI API key in Settings for on-device Grok, or set Backend URL. Hub context is sent when you pick a hub.",
            ),
        )
    }

    func send(baseURL: URL?, selectedHubId: String?) async {
        let trimmed = input.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !trimmed.isEmpty else { return }

        lastError = nil
        let priorHistory = buildChatHistory()
        messages.append(ChatMessage(role: .user, text: trimmed))
        input = ""
        isSending = true
        defer { isSending = false }

        let history = priorHistory

        if let key = XaiKeyStore.shared.apiKey, !key.isEmpty {
            do {
                let reply = try await XaiChatClient.send(
                    apiKey: key,
                    model: xaiModel.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty
                        ? "grok-3-latest" : xaiModel,
                    userMessage: trimmed,
                    chatHistory: history,
                    selectedHubId: selectedHubId,
                )
                messages.append(ChatMessage(role: .assistant, text: reply))
            } catch let e as XaiChatClientError {
                lastError = e.localizedDescription
                messages.append(
                    ChatMessage(role: .assistant, text: "Error: \(e.localizedDescription)"),
                )
            } catch {
                lastError = error.localizedDescription
                messages.append(
                    ChatMessage(role: .assistant, text: "Error: \(error.localizedDescription)"),
                )
            }
            return
        }

        guard let baseURL else {
            lastError =
                "Add your xAI API key in Settings (on-device Grok), or set Backend URL for a remote chat server."
            return
        }

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
