import SwiftUI

struct ContentView: View {
    @EnvironmentObject private var settings: AppSettings
    @StateObject private var chat = ChatViewModel()
    @State private var showSettings = false
    @State private var showLogin = false

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
                        Task { await chat.send(baseURL: settings.baseURL) }
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
                    Button("Sign in") {
                        showLogin = true
                    }
                }
                ToolbarItem(placement: .topBarTrailing) {
                    Button {
                        showSettings = true
                    } label: {
                        Image(systemName: "gearshape")
                    }
                }
            }
            .sheet(isPresented: $showSettings) {
                NavigationStack {
                    Form {
                        Section {
                            TextField("Base URL", text: $settings.baseURLString)
                                .textInputAutocapitalization(.never)
                                .autocorrectionDisabled()
                                #if os(iOS)
                                    .keyboardType(.URL)
                                #endif
                        } header: {
                            Text("aps-ai-web")
                        } footer: {
                            Text(
                                "Use http://127.0.0.1:3000 in Simulator. On a physical device, use your Mac’s LAN IP (for example http://192.168.1.10:3000) with `npm run dev` bound to 0.0.0.0.",
                            )
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
            .sheet(isPresented: $showLogin) {
                if let start = settings.baseURL.flatMap({ URL(string: "/auth/login", relativeTo: $0)?.absoluteURL }) {
                    NavigationStack {
                        LoginWebView(startURL: start) {
                            showLogin = false
                        }
                        .ignoresSafeArea()
                        .navigationTitle("Sign in")
                        .navigationBarTitleDisplayMode(.inline)
                        .toolbar {
                            ToolbarItem(placement: .cancellationAction) {
                                Button("Close") { showLogin = false }
                            }
                        }
                    }
                } else {
                    Text("Set a valid Base URL in Settings first.")
                        .padding()
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
}
