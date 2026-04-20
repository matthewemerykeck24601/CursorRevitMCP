import AuthenticationServices
import Foundation
import SwiftUI
import UIKit

enum NativeAuthError: LocalizedError {
    case missingClientId
    case buildAuthURLFailed
    case missingCodeOrState
    case oauthError(String)
    case sessionStartFailed
    case tokenExchange(Error)

    var errorDescription: String? {
        switch self {
        case .missingClientId:
            return "Set AUTODESK_CLIENT_ID in Info.plist (APS app Client ID)."
        case .buildAuthURLFailed:
            return "Could not build Autodesk authorize URL."
        case .missingCodeOrState:
            return "OAuth callback missing code."
        case .oauthError(let s):
            return s
        case .sessionStartFailed:
            return "Could not start sign-in session."
        case .tokenExchange(let e):
            return e.localizedDescription
        }
    }
}

/// Native Autodesk OAuth (PKCE) + **on-device** token exchange → Keychain. No local Node server.
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

    func signIn() async throws {
        guard let clientId = AutodeskOAuthConfig.clientId else {
            throw NativeAuthError.missingClientId
        }
        let redirectUri = AutodeskOAuthConfig.redirectUri
        let verifier = PKCE.generateCodeVerifier()
        let challenge = PKCE.codeChallengeS256(verifier: verifier)
        let state = UUID().uuidString

        guard var components = URLComponents(string: AutodeskOAuthConfig.authorizeEndpoint) else {
            throw NativeAuthError.buildAuthURLFailed
        }
        components.queryItems = [
            URLQueryItem(name: "response_type", value: "code"),
            URLQueryItem(name: "client_id", value: clientId),
            URLQueryItem(name: "redirect_uri", value: redirectUri),
            URLQueryItem(name: "scope", value: AutodeskOAuthConfig.scope),
            URLQueryItem(name: "state", value: state),
            URLQueryItem(name: "code_challenge", value: challenge),
            URLQueryItem(name: "code_challenge_method", value: "S256"),
        ]
        guard let authURL = components.url else {
            throw NativeAuthError.buildAuthURLFailed
        }

        let scheme = URL(string: redirectUri)?.scheme ?? "monty"

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
                            redirectUri: redirectUri,
                            clientId: clientId,
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
        clientId: String,
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

        do {
            let tokens = try await AutodeskDirectOAuth.exchangeCodeForTokens(
                code: code,
                codeVerifier: codeVerifier,
                redirectUri: redirectUri,
                clientId: clientId,
            )
            AutodeskTokenStore.shared.save(tokens: tokens)
        } catch {
            throw NativeAuthError.tokenExchange(error)
        }
    }
}
