import Foundation
import SwiftUI

/// First-run: Autodesk sign-in (Keychain) → hub. Optional **Backend URL** only for chat / admin API proxy.
@MainActor
final class AppCoordinator: ObservableObject {
    enum Phase: Equatable {
        case bootstrapping
        case needsSignIn
        case needsHub
        case main
    }

    @Published var phase: Phase = .bootstrapping
    @Published var bootstrapMessage: String?

    func bootstrap(settings: AppSettings) async {
        phase = .bootstrapping
        bootstrapMessage = nil

        let hasInfoPlistClientId = AutodeskOAuthConfig.clientId != nil
        let hasBackendUrl = settings.baseURL != nil
        guard hasInfoPlistClientId || hasBackendUrl else {
            bootstrapMessage = "Set AUTODESK_CLIENT_ID in Info.plist or set Backend URL to use hosted auth config."
            phase = .needsSignIn
            return
        }

        let store = AutodeskTokenStore.shared
        if await store.ensureValidAccessToken() {
            applyPostAuth(settings: settings)
        } else {
            if store.accessToken != nil {
                store.clear()
            }
            phase = .needsSignIn
        }
    }

    func afterSignInAttempt(settings: AppSettings) async {
        bootstrapMessage = nil
        applyPostAuth(settings: settings)
    }

    private func applyPostAuth(settings: AppSettings) {
        if settings.selectedHubId != nil {
            phase = .main
        } else {
            phase = .needsHub
        }
    }

    func hubSelectionComplete(settings: AppSettings) {
        guard settings.selectedHubId != nil else { return }
        phase = .main
    }

    func sessionExpired() {
        AutodeskTokenStore.shared.clear()
        SessionCookieSync.clearSessionCookiesFromSharedStorage()
        phase = .needsSignIn
        bootstrapMessage = "Your Autodesk session expired. Please sign in again."
    }

    func openHubPicker() {
        phase = .needsHub
    }
}
