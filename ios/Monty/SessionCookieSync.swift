import Foundation
import WebKit

enum SessionCookieSync {
    private static let sessionNames: Set<String> = [
        "aps_access_token",
        "aps_refresh_token",
        "aps_expires_at",
        "aps_scope",
    ]

    /// Copies APS session cookies from the WKWebView store into shared `HTTPCookieStorage` for API calls.
    static func syncFromWebView(_ webView: WKWebView, completion: @escaping () -> Void) {
        let store = webView.configuration.websiteDataStore.httpCookieStore
        store.getAllCookies { cookies in
            let storage = HTTPCookieStorage.shared
            for cookie in cookies where sessionNames.contains(cookie.name) {
                storage.setCookie(cookie)
            }
            DispatchQueue.main.async { completion() }
        }
    }
}
