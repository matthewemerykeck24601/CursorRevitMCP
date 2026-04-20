import Foundation
import Security

enum MontyTokenAccounts {
    static let access = "monty.aps.accessToken"
    static let refresh = "monty.aps.refreshToken"
    static let expiry = "monty.aps.expiresAtMs"
}

/// Stores APS access/refresh tokens in the Keychain (standalone auth — no local Node server).
final class AutodeskTokenStore: ObservableObject {
    static let shared = AutodeskTokenStore()

    @Published private(set) var isSignedIn: Bool = false

    private init() {
        isSignedIn = loadAccessToken() != nil
    }

    var accessToken: String? { loadAccessToken() }
    var refreshToken: String? { KeychainHelper.load(account: MontyTokenAccounts.refresh) }

    private func loadAccessToken() -> String? {
        KeychainHelper.load(account: MontyTokenAccounts.access)
    }

    private var expiresAtMs: Double? {
        guard let s = KeychainHelper.load(account: MontyTokenAccounts.expiry), let v = Double(s) else { return nil }
        return v
    }

    /// Returns true if a non-expired access token is available (refreshes on device when needed).
    func ensureValidAccessToken() async -> Bool {
        guard loadAccessToken() != nil else { return false }
        guard let exp = expiresAtMs else { return true }
        let expiryDate = Date(timeIntervalSince1970: exp / 1000.0)
        if Date() < expiryDate.addingTimeInterval(-120) {
            return true
        }
        guard let rt = refreshToken, !rt.isEmpty, let cid = AutodeskOAuthConfig.clientId else {
            return false
        }
        do {
            let tokens = try await AutodeskDirectOAuth.refreshTokens(
                refreshToken: rt,
                clientId: cid,
                scope: AutodeskOAuthConfig.scope,
            )
            save(tokens: tokens)
            return true
        } catch {
            return false
        }
    }

    func save(tokens: AutodeskTokenResponse) {
        KeychainHelper.save(tokens.access_token, account: MontyTokenAccounts.access)
        if let rt = tokens.refresh_token, !rt.isEmpty {
            KeychainHelper.save(rt, account: MontyTokenAccounts.refresh)
        }
        let expMs = Date().timeIntervalSince1970 * 1000 + Double(tokens.expires_in) * 1000
        KeychainHelper.save(String(expMs), account: MontyTokenAccounts.expiry)
        DispatchQueue.main.async {
            self.isSignedIn = true
        }
    }

    func clear() {
        KeychainHelper.delete(account: MontyTokenAccounts.access)
        KeychainHelper.delete(account: MontyTokenAccounts.refresh)
        KeychainHelper.delete(account: MontyTokenAccounts.expiry)
        DispatchQueue.main.async {
            self.isSignedIn = false
        }
    }
}

enum KeychainHelper {
    private static let service = "com.cursorrevitmcp.monty.tokens"

    static func save(_ value: String, account: String) {
        delete(account: account)
        let data = Data(value.utf8)
        let query: [String: Any] = [
            kSecClass as String: kSecClassGenericPassword,
            kSecAttrService as String: service,
            kSecAttrAccount as String: account,
            kSecValueData as String: data,
            kSecAttrAccessible as String: kSecAttrAccessibleAfterFirstUnlockThisDeviceOnly,
        ]
        SecItemAdd(query as CFDictionary, nil)
    }

    static func load(account: String) -> String? {
        let query: [String: Any] = [
            kSecClass as String: kSecClassGenericPassword,
            kSecAttrService as String: service,
            kSecAttrAccount as String: account,
            kSecReturnData as String: true,
            kSecMatchLimit as String: kSecMatchLimitOne,
        ]
        var out: AnyObject?
        let status = SecItemCopyMatching(query as CFDictionary, &out)
        guard status == errSecSuccess, let data = out as? Data else { return nil }
        return String(data: data, encoding: .utf8)
    }

    static func delete(account: String) {
        let query: [String: Any] = [
            kSecClass as String: kSecClassGenericPassword,
            kSecAttrService as String: service,
            kSecAttrAccount as String: account,
        ]
        SecItemDelete(query as CFDictionary)
    }
}
