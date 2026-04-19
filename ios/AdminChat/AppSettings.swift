import Foundation

/// Persisted base URL for `aps-ai-web` (e.g. http://127.0.0.1:3000 or your LAN IP for a device).
final class AppSettings: ObservableObject {
    private let defaults = UserDefaults.standard
    private let baseURLKey = "adminChat.baseURL"

    @Published var baseURLString: String {
        didSet {
            let trimmed = baseURLString.trimmingCharacters(in: .whitespacesAndNewlines)
            if trimmed != baseURLString {
                baseURLString = trimmed
                return
            }
            defaults.set(baseURLString, forKey: baseURLKey)
        }
    }

    init() {
        let initial = defaults.string(forKey: baseURLKey) ?? "http://127.0.0.1:3000"
        _baseURLString = Published(initialValue: initial)
    }

    var baseURL: URL? {
        let s = baseURLString.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !s.isEmpty, let u = URL(string: s), u.scheme != nil else { return nil }
        return u
    }
}
