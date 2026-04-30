import SwiftUI

struct SignInGateView: View {
    @EnvironmentObject private var settings: AppSettings
    @EnvironmentObject private var coordinator: AppCoordinator
    @State private var nativeAuthCoordinator = AutodeskNativeAuthCoordinator()
    @State private var isSigningIn = false

    var body: some View {
        NavigationStack {
            VStack(spacing: 24) {
                Image(systemName: "person.badge.key.fill")
                    .font(.system(size: 56))
                    .foregroundStyle(.tint)

                Text("Sign in with Autodesk")
                    .font(.title2.weight(.semibold))

                if let msg = coordinator.bootstrapMessage, !msg.isEmpty {
                    Text(msg)
                        .font(.footnote)
                        .foregroundStyle(.secondary)
                        .multilineTextAlignment(.center)
                        .padding(.horizontal)
                }

                Button {
                    Task { await runNativeSignIn() }
                } label: {
                    if isSigningIn {
                        ProgressView()
                            .frame(maxWidth: .infinity)
                    } else {
                        Text("Sign in with Autodesk")
                            .frame(maxWidth: .infinity)
                    }
                }
                .buttonStyle(.borderedProminent)
                .disabled(isSigningIn)
                .padding(.horizontal)

                Text(
                    "Uses AUTODESK_CLIENT_ID from Info.plist, or fetches native auth config from Backend URL. Redirect is monty://autodesk-oauth (PKCE).",
                )
                .font(.footnote)
                .foregroundStyle(.secondary)
                .multilineTextAlignment(.center)
                .padding(.horizontal)

                Spacer()
            }
            .padding(.top, 48)
            .navigationTitle("Monty")
        }
    }

    @MainActor
    private func runNativeSignIn() async {
        if AutodeskOAuthConfig.clientId == nil && settings.baseURL == nil {
            coordinator.bootstrapMessage = "Set AUTODESK_CLIENT_ID in Info.plist or set Backend URL."
            return
        }
        isSigningIn = true
        coordinator.bootstrapMessage = nil
        defer { isSigningIn = false }

        do {
            try await nativeAuthCoordinator.signIn(baseURL: settings.baseURL)
            await coordinator.afterSignInAttempt(settings: settings)
        } catch is CancellationError {
            // dismissed browser
        } catch {
            coordinator.bootstrapMessage = error.localizedDescription
        }
    }
}

struct HubSelectionView: View {
    @EnvironmentObject private var settings: AppSettings
    @EnvironmentObject private var coordinator: AppCoordinator

    @State private var hubs: [HubInfo] = []
    @State private var isLoading = true
    @State private var loadError: String?
    @State private var selectedId: String?

    var body: some View {
        NavigationStack {
            Group {
                if isLoading {
                    ProgressView("Loading hubs…")
                } else if let loadError {
                    ContentUnavailableView(
                        "Couldn’t load hubs",
                        systemImage: "exclamationmark.triangle",
                        description: Text(loadError),
                    )
                } else if hubs.isEmpty {
                    ContentUnavailableView(
                        "No hubs",
                        systemImage: "building.2",
                        description: Text("This Autodesk account has no hubs, or the list is empty."),
                    )
                } else {
                    Form {
                        Section {
                            Picker("Hub", selection: $selectedId) {
                                Text("Choose a hub").tag(Optional<String>.none)
                                ForEach(hubs) { hub in
                                    Text(hub.name).tag(Optional(hub.id))
                                }
                            }
                        } footer: {
                            Text("Admin chat / tools use this hub as context. You can change it later in Settings.")
                        }

                        Section {
                            Button("Use this hub") {
                                applySelection()
                            }
                            .disabled(selectedId == nil)
                        }
                    }
                }
            }
            .navigationTitle("Select hub")
            .task {
                await loadHubs()
            }
            .toolbar {
                if settings.selectedHubId != nil {
                    ToolbarItem(placement: .cancellationAction) {
                        Button("Cancel") {
                            coordinator.hubSelectionComplete(settings: settings)
                        }
                    }
                }
            }
        }
    }

    private func loadHubs() async {
        isLoading = true
        loadError = nil
        defer { isLoading = false }

        guard await AutodeskTokenStore.shared.ensureValidAccessToken() else {
            coordinator.sessionExpired()
            return
        }
        guard let token = AutodeskTokenStore.shared.accessToken else {
            loadError = "Not signed in."
            return
        }

        do {
            let list = try await AutodeskDirectAPI.fetchHubs(accessToken: token)
            hubs = list
            if let existing = settings.selectedHubId, list.contains(where: { $0.id == existing }) {
                selectedId = existing
            } else {
                selectedId = list.first?.id
            }
        } catch let error as APSClientError {
            if case .notAuthenticated = error {
                coordinator.sessionExpired()
            } else {
                loadError = error.localizedDescription
            }
        } catch {
            loadError = error.localizedDescription
        }
    }

    private func applySelection() {
        guard let id = selectedId, let hub = hubs.first(where: { $0.id == id }) else { return }
        settings.setSelectedHub(id: hub.id, name: hub.name)
        coordinator.hubSelectionComplete(settings: settings)
    }
}
