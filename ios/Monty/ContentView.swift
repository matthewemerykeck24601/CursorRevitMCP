import SwiftUI

struct ContentView: View {
    @EnvironmentObject private var settings: AppSettings
    @EnvironmentObject private var coordinator: AppCoordinator
    @StateObject private var chat = ChatViewModel()
    @State private var showSettings = false
    @State private var showAddUsersTool = false

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
                            TextField("Backend URL (optional)", text: $settings.baseURLString)
                                .textInputAutocapitalization(.never)
                                .autocorrectionDisabled()
                                #if os(iOS)
                                    .keyboardType(.URL)
                                #endif
                        } header: {
                            Text("aps-ai-web")
                        } footer: {
                            Text(
                                "Chat: monty-ai-server (Grok), e.g. http://127.0.0.1:8787. Add-users still uses aps-ai-web if you need that. Leave empty for sign-in + hub only.",
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
}
