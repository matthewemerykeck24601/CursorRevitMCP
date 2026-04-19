import AuthenticationServices
import Foundation
import SwiftUI
import UIKit

enum NativeAuthError: LocalizedError {
    case invalidBaseURL
    case configFailed(String)
    case buildAuthURLFailed
    case missingCodeOrState
    case oauthError(String)
    case exchangeFailed(Int, String)
    case sessionStartFailed

    var errorDescription: String? {
        switch self {
        case .invalidBaseURL:
            return "Invalid server URL."
        case .configFailed(let s):
            return s
        case .buildAuthURLFailed:
            return "Could not build Autodesk authorize URL."
        case .missingCodeOrState:
            return "OAuth callback missing code."
        case .oauthError(let s):
            return s
        case .exchangeFailed(let code, let body):
            return "Token exchange failed (\(code)): \(body)"
        case .sessionStartFailed:
            return "Could not start sign-in session."
        }
    }
}

/// Native Autodesk OAuth (PKCE) + server-side code exchange → session cookies for `aps-ai-web`.
@MainActor
final class AutodeskNativeAuthCoordinator: NSObject, ASWebAuthenticationPresentationContextProviding {
    private var authSession: ASWebAuthenticationSession?

    func presentationAnchor(for session: ASWebAuthenticationSession) -> ASPresentationAnchor {
        guard let scene = UIApplication.shared.connectedScenes.first as? UIWindowScene,
              let window = scene.windows.first(where: { $0.isKeyWindow }) ?? scene.windows.first
        else {
            return ASPresentationAnchor()
        }
        return window
    }

    func signIn(baseURL: URL, api: APSAPIClient) async throws {
        let config = try await api.fetchNativeAuthConfiguration(baseURL: baseURL)
        let verifier = PKCE.generateCodeVerifier()
        let challenge = PKCE.codeChallengeS256(verifier: verifier)
        let state = UUID().uuidString

        guard var components = URLComponents(string: config.authorizeEndpoint) else {
            throw NativeAuthError.buildAuthURLFailed
        }
        components.queryItems = [
            URLQueryItem(name: "response_type", value: "code"),
            URLQueryItem(name: "client_id", value: config.clientId),
            URLQueryItem(name: "redirect_uri", value: config.redirectUri),
            URLQueryItem(name: "scope", value: config.scope),
            URLQueryItem(name: "state", value: state),
            URLQueryItem(name: "code_challenge", value: challenge),
            URLQueryItem(name: "code_challenge_method", value: "S256"),
        ]
        guard let authURL = components.url else {
            throw NativeAuthError.buildAuthURLFailed
        }

        let scheme =
            URL(string: config.redirectUri)?.scheme
                ?? "monty"

        try await withCheckedThrowingContinuation { (continuation: CheckedContinuation<Void, Error>) in
            let session = ASWebAuthenticationSession(
                url: authURL,
                callbackURLScheme: scheme,
            ) { [weak self] callbackURL, error in
                guard let self else {
                    continuation.resume(throwing: NativeAuthError.sessionStartFailed)
                    return
                }
                Task { @MainActor in
                    defer { self.authSession = nil }
                    if let error {
                        let ns = error as NSError
                        if ns.domain == ASWebAuthenticationSessionErrorDomain, ns.code == 1 {
                            continuation.resume(throwing: CancellationError())
                            return
                        }
                        continuation.resume(throwing: error)
                        return
                    }
                    guard let callbackURL else {
                        continuation.resume(throwing: NativeAuthError.missingCodeOrState)
                        return
                    }
                    do {
                        try await self.handleCallback(
                            callbackURL: callbackURL,
                            expectedState: state,
                            codeVerifier: verifier,
                            redirectUri: config.redirectUri,
                            baseURL: baseURL,
                            api: api,
                        )
                        continuation.resume()
                    } catch {
                        continuation.resume(throwing: error)
                    }
                }
            }
            session.presentationContextProvider = self
            session.prefersEphemeralWebBrowserSession = false
            self.authSession = session
            if !session.start() {
                continuation.resume(throwing: NativeAuthError.sessionStartFailed)
            }
        }
    }

    private func handleCallback(
        callbackURL: URL,
        expectedState: String,
        codeVerifier: String,
        redirectUri: String,
        baseURL: URL,
        api: APSAPIClient,
    ) async throws {
        let items = URLComponents(url: callbackURL, resolvingAgainstBaseURL: false)?.queryItems ?? []
        let qp = Dictionary(uniqueKeysWithValues: items.map { ($0.name, $0.value ?? "") })

        if let err = qp["error"], !err.isEmpty {
            let desc = qp["error_description"] ?? err
            throw NativeAuthError.oauthError(desc)
        }

        guard let code = qp["code"], !code.isEmpty else {
            throw NativeAuthError.missingCodeOrState
        }
        if let st = qp["state"], !st.isEmpty, st != expectedState {
            throw NativeAuthError.oauthError("Invalid OAuth state")
        }

        try await api.postNativeExchange(
            baseURL: baseURL,
            code: code,
            codeVerifier: codeVerifier,
            redirectUri: redirectUri,
        )
    }
}
