import Foundation
import SwiftUI

/// Drives first-run onboarding (server URL → Autodesk sign-in → hub) and re-auth on 401.
@MainActor
final class AppCoordinator: ObservableObject {
    enum Phase: Equatable {
        case bootstrapping
        case needsBaseURL
        case needsSignIn
        case needsHub
        case main
    }

    @Published var phase: Phase = .bootstrapping
    @Published var bootstrapMessage: String?

    private let api = APSAPIClient()

    func bootstrap(settings: AppSettings) async {
        phase = .bootstrapping
        bootstrapMessage = nil

        guard let base = settings.baseURL else {
            phase = .needsBaseURL
            return
        }

        do {
            let status = try await api.fetchSessionStatus(baseURL: base)
            applySessionStatus(status, settings: settings)
        } catch {
            bootstrapMessage = error.localizedDescription
            phase = .needsSignIn
        }
    }

    func afterSignInAttempt(settings: AppSettings) async {
        bootstrapMessage = nil
        guard let base = settings.baseURL else {
            phase = .needsBaseURL
            return
        }

        do {
            let status = try await api.fetchSessionStatus(baseURL: base)
            applySessionStatus(status, settings: settings)
        } catch {
            bootstrapMessage = error.localizedDescription
            phase = .needsSignIn
        }
    }

    private func applySessionStatus(_ status: SessionStatus, settings: AppSettings) {
        switch status {
        case .unauthenticated:
            phase = .needsSignIn
        case .authenticated:
            if settings.selectedHubId != nil {
                phase = .main
            } else {
                phase = .needsHub
            }
        }
    }

    func continueFromBaseURL(settings: AppSettings) {
        bootstrapMessage = nil
        guard settings.baseURL != nil else {
            phase = .needsBaseURL
            return
        }
        Task { await bootstrap(settings: settings) }
    }

    func hubSelectionComplete(settings: AppSettings) {
        guard settings.selectedHubId != nil else { return }
        phase = .main
    }

    /// Token expired or invalid — send user back to sign-in; hub preference stays in `AppSettings`.
    func sessionExpired() {
        SessionCookieSync.clearSessionCookiesFromSharedStorage()
        phase = .needsSignIn
        bootstrapMessage = "Your Autodesk session expired. Please sign in again."
    }

    func openHubPicker() {
        phase = .needsHub
    }
}
