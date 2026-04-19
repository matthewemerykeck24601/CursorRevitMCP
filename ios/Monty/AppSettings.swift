import Foundation

/// Persisted server URL, selected ACC/BIM hub, and session-related preferences for Monty.
final class AppSettings: ObservableObject {
    private let defaults = UserDefaults.standard
    private let baseURLKey = "monty.baseURL"
    private let hubIdKey = "monty.selectedHubId"
    private let hubNameKey = "monty.selectedHubName"

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

    /// Data Management hub id (e.g. `b.xxx`); required for admin chat context.
    @Published var selectedHubId: String? {
        didSet {
            if let id = selectedHubId, !id.isEmpty {
                defaults.set(id, forKey: hubIdKey)
            } else {
                defaults.removeObject(forKey: hubIdKey)
            }
        }
    }

    @Published var selectedHubName: String? {
        didSet {
            if let name = selectedHubName, !name.isEmpty {
                defaults.set(name, forKey: hubNameKey)
            } else {
                defaults.removeObject(forKey: hubNameKey)
            }
        }
    }

    init() {
        let initial = defaults.string(forKey: baseURLKey) ?? "http://127.0.0.1:3000"
        _baseURLString = Published(initialValue: initial)
        _selectedHubId = Published(initialValue: defaults.string(forKey: hubIdKey))
        _selectedHubName = Published(initialValue: defaults.string(forKey: hubNameKey))
    }

    var baseURL: URL? {
        let s = baseURLString.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !s.isEmpty, let u = URL(string: s), u.scheme != nil else { return nil }
        return u
    }

    func setSelectedHub(id: String, name: String) {
        selectedHubId = id
        selectedHubName = name
    }

    func clearSelectedHub() {
        selectedHubId = nil
        selectedHubName = nil
    }
}
