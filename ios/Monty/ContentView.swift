import SwiftUI

struct ContentView: View {
    @EnvironmentObject private var settings: AppSettings
    @EnvironmentObject private var coordinator: AppCoordinator
    @EnvironmentObject private var xaiKeys: XaiKeyStore
    @StateObject private var chat = ChatViewModel()
    @State private var showSettings = false
    @State private var showAddUsersTool = false
    @State private var xaiKeyDraft = ""

    var body: some View {
        NavigationStack {
            VStack(spacing: 0) {
                ScrollViewReader { proxy in
                    ScrollView {
                        LazyVStack(alignment: .leading, spacing: 12) {
                            ForEach(chat.messages) { msg in
                                messageBubble(msg)
                                    .id(msg.id)
                            }
                        }
                        .padding()
                    }
                    .onChange(of: chat.messages.count) { _, _ in
                        if let last = chat.messages.last {
                            withAnimation {
                                proxy.scrollTo(last.id, anchor: .bottom)
                            }
                        }
                    }
                }

                if let err = chat.lastError {
                    Text(err)
                        .font(.footnote)
                        .foregroundStyle(.red)
                        .frame(maxWidth: .infinity, alignment: .leading)
                        .padding(.horizontal)
                }

                Divider()

                HStack(alignment: .bottom) {
                    TextField("Task (e.g. add user to project…)", text: $chat.input, axis: .vertical)
                        .textFieldStyle(.roundedBorder)
                        .lineLimit(1...6)

                    Button {
                        Task {
                            await chat.send(
                                baseURL: settings.baseURL,
                                selectedHubId: settings.selectedHubId,
                            )
                        }
                    } label: {
                        Image(systemName: "arrow.up.circle.fill")
                            .font(.title2)
                    }
                    .disabled(chat.isSending || chat.input.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty)
                }
                .padding()
            }
            .navigationTitle("Monty")
            .toolbar {
                ToolbarItem(placement: .topBarLeading) {
                    Button {
                        showAddUsersTool = true
                    } label: {
                        Image(systemName: "person.badge.plus")
                    }
                    .accessibilityLabel("Add users to projects")
                }
                ToolbarItem(placement: .topBarTrailing) {
                    Button {
                        showSettings = true
                    } label: {
                        Image(systemName: "gearshape")
                    }
                }
            }
            .sheet(isPresented: $showAddUsersTool) {
                AddUsersToProjectsView()
                    .environmentObject(settings)
            }
            .sheet(isPresented: $showSettings) {
                NavigationStack {
                    Form {
                        Section {
                            if xaiKeys.hasKey {
                                Text("xAI API key saved on this device (Keychain).")
                                    .foregroundStyle(.secondary)
                                    .font(.footnote)
                                Button("Remove xAI key", role: .destructive) {
                                    xaiKeys.saveKey("")
                                }
                            }
                            SecureField("Paste xAI API key (console.x.ai)", text: $xaiKeyDraft)
                                .textInputAutocapitalization(.never)
                                .autocorrectionDisabled()
                            Button("Save xAI key") {
                                xaiKeys.saveKey(xaiKeyDraft)
                                xaiKeyDraft = ""
                            }
                            .disabled(xaiKeyDraft.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty)
                            TextField("xAI model id", text: $settings.xaiModel)
                                .textInputAutocapitalization(.never)
                                .autocorrectionDisabled()
                        } header: {
                            Text("On-device Grok (recommended)")
                        } footer: {
                            Text(
                                "APS Client ID belongs in Info.plist (public OAuth id — that is normal). The xAI key stays in Keychain, not in the plist. Leave Backend URL empty to use Grok from your phone anywhere.",
                            )
                        }

                        Section {
                            TextField("Backend URL (optional)", text: $settings.baseURLString)
                                .textInputAutocapitalization(.never)
                                .autocorrectionDisabled()
                                #if os(iOS)
                                    .keyboardType(.URL)
                                #endif
                        } header: {
                            Text("Remote server")
                        } footer: {
                            Text(
                                "Optional: monty-ai-server or deployed aps-ai-web if you prefer server-side chat or web parity. On-device xAI takes priority when a key is saved.",
                            )
                        }

                        Section {
                            LabeledContent("Hub") {
                                Text(settings.selectedHubName ?? settings.selectedHubId ?? "—")
                                    .foregroundStyle(.secondary)
                                    .multilineTextAlignment(.trailing)
                            }
                            Button("Change hub…") {
                                showSettings = false
                                coordinator.openHubPicker()
                            }
                        } header: {
                            Text("Autodesk")
                        }

                        Section {
                            Button("Sign out", role: .destructive) {
                                showSettings = false
                                coordinator.sessionExpired()
                            }
                        } footer: {
                            Text("Clears Autodesk tokens from this device. Your saved hub stays selected for next sign-in.")
                        }
                    }
                    .navigationTitle("Settings")
                    .toolbar {
                        ToolbarItem(placement: .confirmationAction) {
                            Button("Done") { showSettings = false }
                        }
                    }
                }
            }
        }
    }

    @ViewBuilder
    private func messageBubble(_ msg: ChatMessage) -> some View {
        switch msg.role {
        case .system:
            Text(msg.text)
                .font(.footnote)
                .foregroundStyle(.secondary)
                .padding(10)
                .frame(maxWidth: .infinity, alignment: .leading)
                .background(Color(.secondarySystemBackground))
                .clipShape(RoundedRectangle(cornerRadius: 10))
        case .user:
            Text(msg.text)
                .padding(10)
                .frame(maxWidth: .infinity, alignment: .trailing)
                .background(Color.accentColor.opacity(0.2))
                .clipShape(RoundedRectangle(cornerRadius: 10))
        case .assistant:
            Text(msg.text)
                .font(.body)
                .padding(10)
                .frame(maxWidth: .infinity, alignment: .leading)
                .background(Color(.secondarySystemBackground))
                .clipShape(RoundedRectangle(cornerRadius: 10))
        }
    }
}

#Preview {
    ContentView()
        .environmentObject(AppSettings())
        .environmentObject(AppCoordinator())
        .environmentObject(XaiKeyStore.shared)
}
