import SwiftUI
import WebKit

struct LoginWebView: UIViewRepresentable {
    let startURL: URL
    var onCookiesSynced: () -> Void

    func makeCoordinator() -> Coordinator {
        Coordinator(parent: self)
    }

    func makeUIView(context: Context) -> WKWebView {
        let config = WKWebViewConfiguration()
        config.websiteDataStore = .default()
        let webView = WKWebView(frame: .zero, configuration: config)
        webView.navigationDelegate = context.coordinator
        webView.load(URLRequest(url: startURL))
        return webView
    }

    func updateUIView(_ webView: WKWebView, context: Context) {
        context.coordinator.parent = self
    }

    final class Coordinator: NSObject, WKNavigationDelegate {
        var parent: LoginWebView

        init(parent: LoginWebView) {
            self.parent = parent
        }

        func webView(_ webView: WKWebView, decidePolicyFor navigationAction: WKNavigationAction) async -> WKNavigationActionPolicy {
            .allow
        }

        func webView(_ webView: WKWebView, didFinish navigation: WKNavigation!) {
            guard let url = webView.url else { return }
            let path = url.path
            // Avoid firing during initial `/auth/login` load; run after callback or app home.
            let shouldSync =
                path.hasPrefix("/auth/callback") || path == "/" || path.isEmpty
            guard shouldSync else { return }
            SessionCookieSync.syncFromWebView(webView) {
                self.parent.onCookiesSynced()
            }
        }
    }
}
