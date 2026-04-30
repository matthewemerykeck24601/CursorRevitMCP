import Foundation

/// **Backend URL** is optional if you use on-device xAI + ACC admin. Auth + hubs use Autodesk directly.
final class AppSettings: ObservableObject {
    private let defaults = UserDefaults.standard
    private let baseURLKey = "monty.backendBaseURL"
    private let hubIdKey = "monty.selectedHubId"
    private let hubNameKey = "monty.selectedHubName"
    private let xaiModelKey = "monty.xaiModel"

    /// e.g. `https://your-app.vercel.app` or `http://127.0.0.1:3000` — **leave empty** if you only use sign-in + hubs.
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

    /// xAI model id for on-device Grok chat (see console.x.ai).
    @Published var xaiModel: String {
        didSet {
            let t = xaiModel.trimmingCharacters(in: .whitespacesAndNewlines)
            if t != xaiModel {
                xaiModel = t
                return
            }
            defaults.set(xaiModel, forKey: xaiModelKey)
        }
    }

    init() {
        _baseURLString = Published(initialValue: defaults.string(forKey: baseURLKey) ?? "")
        _selectedHubId = Published(initialValue: defaults.string(forKey: hubIdKey))
        _selectedHubName = Published(initialValue: defaults.string(forKey: hubNameKey))
        _xaiModel = Published(initialValue: defaults.string(forKey: xaiModelKey) ?? "grok-3-latest")
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
