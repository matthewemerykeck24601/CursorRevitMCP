import SwiftUI

/// Structured ACC admin: add users by project number (server: `addUsersToProjectsByNumber` — same as web/chat, no MCP on device).
struct AddUsersToProjectsView: View {
    @Environment(\.dismiss) private var dismiss
    @EnvironmentObject private var settings: AppSettings

    @State private var projectNumbersText = ""
    @State private var emailsText = ""
    @State private var roleNamesText = ""
    @State private var region: Region = .us
    @State private var dryRun = true
    @State private var isSubmitting = false
    @State private var resultText: String?
    @State private var errorText: String?

    private let api = APSAPIClient()

    private enum Region: String, CaseIterable, Identifiable {
        case us = "US"
        case emea = "EMEA"
        var id: String { rawValue }
    }

    var body: some View {
        NavigationStack {
            Form {
                Section {
                    TextField("e.g. JOB12345, JOB99", text: $projectNumbersText, axis: .vertical)
                        .lineLimit(3...8)
                    TextField("user@company.com", text: $emailsText, axis: .vertical)
                        .lineLimit(2...8)
                        .textInputAutocapitalization(.never)
                        .autocorrectionDisabled()
                        .keyboardType(.emailAddress)
                    TextField("Optional roles (comma-separated)", text: $roleNamesText)
                        .textInputAutocapitalization(.never)
                } header: {
                    Text("Targets")
                } footer: {
                    Text("Project numbers and emails can be separated by commas, semicolons, or new lines. Matches the hub you set at sign-in (\(settings.selectedHubName ?? settings.selectedHubId ?? "—")).")
                }

                Section {
                    Picker("Region", selection: $region) {
                        ForEach(Region.allCases) { r in
                            Text(r.rawValue).tag(r)
                        }
                    }
                    Toggle("Dry run (no writes)", isOn: $dryRun)
                } footer: {
                    Text("Dry run previews payloads and resolves projects from the hub cache. Turn off to add users for real.")
                }

                Section {
                    Button {
                        Task { await submit() }
                    } label: {
                        if isSubmitting {
                            ProgressView()
                        } else {
                            Text("Run")
                        }
                    }
                    .disabled(isSubmitting || !canSubmit)
                }

                if let errorText {
                    Section {
                        Text(errorText)
                            .foregroundStyle(.red)
                            .font(.footnote)
                    }
                }

                if let resultText {
                    Section {
                        ScrollView {
                            Text(resultText)
                                .font(.system(.footnote, design: .monospaced))
                                .textSelection(.enabled)
                                .frame(maxWidth: .infinity, alignment: .leading)
                        }
                        .frame(minHeight: 200)
                    } header: {
                        Text("Result")
                    }
                }
            }
            .navigationTitle("Add users to projects")
            .navigationBarTitleDisplayMode(.inline)
            .toolbar {
                ToolbarItem(placement: .cancellationAction) {
                    Button("Close") { dismiss() }
                }
            }
        }
    }

    private var canSubmit: Bool {
        !splitList(projectNumbersText).isEmpty && !splitList(emailsText).isEmpty
    }

    private func splitList(_ s: String) -> [String] {
        s
            .split { $0.isNewline || $0 == "," || $0 == ";" }
            .map { $0.trimmingCharacters(in: .whitespacesAndNewlines) }
            .filter { !$0.isEmpty }
    }

    private func submit() async {
        errorText = nil
        resultText = nil
        guard let base = settings.baseURL else {
            errorText = "Set Backend URL in Settings (your aps-ai-web base URL)."
            return
        }
        guard let hubId = settings.selectedHubId, !hubId.isEmpty else {
            errorText = "No hub selected. Use Settings → Change hub."
            return
        }

        let projects = splitList(projectNumbersText)
        let emails = splitList(emailsText)
        let roles = splitList(roleNamesText)

        isSubmitting = true
        defer { isSubmitting = false }

        do {
            let out = try await api.addUsersToProjects(
                baseURL: base,
                hubId: hubId,
                projectNumbers: projects,
                emails: emails,
                roleNames: roles,
                region: region.rawValue,
                dryRun: dryRun,
            )
            resultText = out
        } catch let error as APSClientError {
            if case .notAuthenticated = error {
                NotificationCenter.default.post(name: .montySessionExpired, object: nil)
                dismiss()
            }
            errorText = error.localizedDescription
        } catch {
            errorText = error.localizedDescription
        }
    }
}

#Preview {
    AddUsersToProjectsView()
        .environmentObject(AppSettings())
}
