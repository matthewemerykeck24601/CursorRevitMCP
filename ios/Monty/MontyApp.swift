import SwiftUI

@main
struct MontyApp: App {
    @StateObject private var settings = AppSettings()
    @StateObject private var coordinator = AppCoordinator()

    var body: some Scene {
        WindowGroup {
            RootView()
                .environmentObject(settings)
                .environmentObject(coordinator)
                .environmentObject(XaiKeyStore.shared)
        }
    }
}
