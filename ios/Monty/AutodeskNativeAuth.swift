import AuthenticationServices
import Foundation
import SwiftUI
import UIKit

private struct NativeAuthRuntimeConfig: Decodable {
    let clientId: String
    let redirectUri: String
    let scope: String
    let authorizeEndpoint: String
}

private struct NativeAuthPublicConfigResponse: Decodable {
    let clientId: String?
    let redirectUri: String?
    let scope: String?
    let authorizeEndpoint: String?
}

private struct NativeBackendExchangeResponse: Decodable {
    let accessToken: String?
    let refreshToken: String?
    let expiresIn: Int?
    let scope: String?
    let error: String?
}

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
            return "Set AUTODESK_CLIENT_ID in Info.plist or set Backend URL so Monty can fetch native auth config."
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

    func signIn(baseURL: URL?) async throws {
        let runtime = try await resolveRuntimeConfig(baseURL: baseURL)
        let clientId = runtime.clientId
        let redirectUri = runtime.redirectUri
        let verifier = PKCE.generateCodeVerifier()
        let challenge = PKCE.codeChallengeS256(verifier: verifier)
        let state = UUID().uuidString

        guard var components = URLComponents(string: runtime.authorizeEndpoint) else {
            throw NativeAuthError.buildAuthURLFailed
        }
        components.queryItems = [
            URLQueryItem(name: "response_type", value: "code"),
            URLQueryItem(name: "client_id", value: clientId),
            URLQueryItem(name: "redirect_uri", value: redirectUri),
            URLQueryItem(name: "scope", value: runtime.scope),
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
                            baseURL: baseURL,
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

    private func resolveRuntimeConfig(baseURL: URL?) async throws -> NativeAuthRuntimeConfig {
        if let clientId = AutodeskOAuthConfig.clientId {
            return NativeAuthRuntimeConfig(
                clientId: clientId,
                redirectUri: AutodeskOAuthConfig.redirectUri,
                scope: AutodeskOAuthConfig.scope,
                authorizeEndpoint: AutodeskOAuthConfig.authorizeEndpoint,
            )
        }
        guard let baseURL else {
            throw NativeAuthError.missingClientId
        }
        guard let url = URL(string: "api/auth/native-config", relativeTo: baseURL)?.absoluteURL else {
            throw NativeAuthError.missingClientId
        }

        let (data, response) = try await URLSession.shared.data(from: url)
        guard let http = response as? HTTPURLResponse, http.statusCode == 200 else {
            throw NativeAuthError.missingClientId
        }
        let decoded = try JSONDecoder().decode(NativeAuthPublicConfigResponse.self, from: data)
        let clientId = (decoded.clientId ?? "").trimmingCharacters(in: .whitespacesAndNewlines)
        let redirectUri = (decoded.redirectUri ?? "").trimmingCharacters(in: .whitespacesAndNewlines)
        let scope = (decoded.scope ?? "").trimmingCharacters(in: .whitespacesAndNewlines)
        let authorizeEndpoint = (decoded.authorizeEndpoint ?? "").trimmingCharacters(in: .whitespacesAndNewlines)

        guard !clientId.isEmpty, !redirectUri.isEmpty, !scope.isEmpty, !authorizeEndpoint.isEmpty else {
            throw NativeAuthError.missingClientId
        }
        return NativeAuthRuntimeConfig(
            clientId: clientId,
            redirectUri: redirectUri,
            scope: scope,
            authorizeEndpoint: authorizeEndpoint,
        )
    }

    private func handleCallback(
        callbackURL: URL,
        expectedState: String,
        codeVerifier: String,
        redirectUri: String,
        clientId: String,
        baseURL: URL?,
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
            if let baseURL {
                do {
                    let tokens = try await exchangeViaBackend(
                        baseURL: baseURL,
                        code: code,
                        codeVerifier: codeVerifier,
                        redirectUri: redirectUri,
                    )
                    AutodeskTokenStore.shared.save(tokens: tokens)
                    return
                } catch {
                    throw NativeAuthError.tokenExchange(error)
                }
            }
            throw NativeAuthError.tokenExchange(error)
        }
    }

    private func exchangeViaBackend(
        baseURL: URL,
        code: String,
        codeVerifier: String,
        redirectUri: String,
    ) async throws -> AutodeskTokenResponse {
        guard let url = URL(string: "api/auth/native-exchange", relativeTo: baseURL)?.absoluteURL else {
            throw NativeAuthError.sessionStartFailed
        }
        var request = URLRequest(url: url)
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        request.httpBody = try JSONSerialization.data(withJSONObject: [
            "code": code,
            "codeVerifier": codeVerifier,
            "redirectUri": redirectUri,
        ])

        let (data, response) = try await URLSession.shared.data(for: request)
        guard let http = response as? HTTPURLResponse else {
            throw NativeAuthError.sessionStartFailed
        }
        let payload = try JSONDecoder().decode(NativeBackendExchangeResponse.self, from: data)
        if http.statusCode != 200 {
            throw NativeAuthError.oauthError(payload.error ?? "Backend token exchange failed.")
        }
        let access = (payload.accessToken ?? "").trimmingCharacters(in: .whitespacesAndNewlines)
        guard !access.isEmpty else {
            throw NativeAuthError.oauthError("Backend token exchange returned no access token.")
        }
        return AutodeskTokenResponse(
            access_token: access,
            token_type: "Bearer",
            expires_in: payload.expiresIn ?? 3600,
            refresh_token: payload.refreshToken,
            scope: payload.scope,
        )
    }
}
