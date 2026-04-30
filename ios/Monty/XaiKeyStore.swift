import Foundation

/// Persists the xAI API key in Keychain so chat works without any Mac or backend.
final class XaiKeyStore: ObservableObject {
    static let shared = XaiKeyStore()

    @Published private(set) var hasKey: Bool

    private init() {
        let k = KeychainHelper.load(account: MontyTokenAccounts.xaiApiKey)
        hasKey = (k?.isEmpty == false)
    }

    var apiKey: String? {
        let k = KeychainHelper.load(account: MontyTokenAccounts.xaiApiKey)
        guard let k, !k.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty else { return nil }
        return k.trimmingCharacters(in: .whitespacesAndNewlines)
    }

    func saveKey(_ raw: String) {
        let t = raw.trimmingCharacters(in: .whitespacesAndNewlines)
        if t.isEmpty {
            KeychainHelper.delete(account: MontyTokenAccounts.xaiApiKey)
            hasKey = false
        } else {
            KeychainHelper.save(t, account: MontyTokenAccounts.xaiApiKey)
            hasKey = true
        }
    }
}
