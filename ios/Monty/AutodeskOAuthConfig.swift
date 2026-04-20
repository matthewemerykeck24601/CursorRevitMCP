import Foundation

/// OAuth settings read from Info.plist (same pattern as CastCam). Set `AUTODESK_CLIENT_ID` in Xcode.
enum AutodeskOAuthConfig {
    /// Must match your APS app; 3-legged scopes (no `code:all`).
    static let defaultScope =
        "data:read data:write data:create data:search user:read offline_access viewables:read account:read"

    static var clientId: String? {
        let v = Bundle.main.object(forInfoDictionaryKey: "AUTODESK_CLIENT_ID") as? String
        let trimmed = v?.trimmingCharacters(in: .whitespacesAndNewlines) ?? ""
        if trimmed.isEmpty || trimmed.hasPrefix("YOUR_") { return nil }
        return trimmed
    }

    static var redirectUri: String {
        (Bundle.main.object(forInfoDictionaryKey: "AUTODESK_REDIRECT_URI") as? String)?.trimmingCharacters(in: .whitespacesAndNewlines).nonEmpty
            ?? "monty://autodesk-oauth"
    }

    static var scope: String {
        (Bundle.main.object(forInfoDictionaryKey: "AUTODESK_SCOPE") as? String)?.trimmingCharacters(in: .whitespacesAndNewlines).nonEmpty
            ?? defaultScope
    }

    static let authorizeEndpoint = "https://developer.api.autodesk.com/authentication/v2/authorize"
    static let tokenEndpoint = "https://developer.api.autodesk.com/authentication/v2/token"
}

private extension String {
    var nonEmpty: String? {
        isEmpty ? nil : self
    }
}
