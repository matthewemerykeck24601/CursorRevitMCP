import SwiftUI

struct RootView: View {
    @EnvironmentObject private var settings: AppSettings
    @EnvironmentObject private var coordinator: AppCoordinator

    var body: some View {
        Group {
            switch coordinator.phase {
            case .bootstrapping:
                ProgressView("Starting…")
                    .frame(maxWidth: .infinity, maxHeight: .infinity)
            case .needsBaseURL:
                BaseURLSetupView()
            case .needsSignIn:
                SignInGateView()
            case .needsHub:
                HubSelectionView()
            case .main:
                ContentView()
            }
        }
        .animation(.easeInOut(duration: 0.2), value: coordinator.phase)
        .task {
            await coordinator.bootstrap(settings: settings)
        }
        .onReceive(NotificationCenter.default.publisher(for: .montySessionExpired)) { _ in
            coordinator.sessionExpired()
        }
    }
}
