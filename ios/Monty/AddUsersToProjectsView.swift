import SwiftUI

/// Structured ACC admin: add users by project number — **on-device** (Construction Admin API) or optional **aps-ai-web** when Backend URL is set.
struct AddUsersToProjectsView: View {
    @Environment(\.dismiss) private var dismiss
    @EnvironmentObject private var settings: AppSettings

    @State private var projectNumbersText = ""
    @State private var emailsText = ""
    @State private var roleNamesText = ""
    /// On-device: comma-separated role **UUIDs** (names are not resolved without the web cache).
    @State private var roleIdsText = ""
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
                    TextField("Optional role names (remote server only)", text: $roleNamesText)
                        .textInputAutocapitalization(.never)
                    TextField("Role IDs — UUIDs, on-device", text: $roleIdsText)
                        .textInputAutocapitalization(.never)
                } header: {
                    Text("Targets")
                } footer: {
                    Text(
                        "Project numbers / emails: commas, semicolons, or new lines. Hub: \(settings.selectedHubName ?? settings.selectedHubId ?? "—"). On **this phone** without Backend URL, use Role IDs (ACC exposes UUIDs in admin); role names are only resolved on the web app.",
                    )
                }

                Section {
                    Picker("Region", selection: $region) {
                        ForEach(Region.allCases) { r in
                            Text(r.rawValue).tag(r)
                        }
                    }
                    Toggle("Dry run (no writes)", isOn: $dryRun)
                } footer: {
                    Text("Dry run previews payloads. On-device, projects are listed live from your hub (same numbering heuristic as the web app).")
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

    private func parseRoleIds(_ s: String) -> [String] {
        splitList(s).compactMap { raw in
            let t = raw.trimmingCharacters(in: .whitespacesAndNewlines)
            guard UUID(uuidString: t) != nil else { return nil }
            return t
        }
    }

    private func submit() async {
        errorText = nil
        resultText = nil
        guard let hubId = settings.selectedHubId, !hubId.isEmpty else {
            errorText = "No hub selected. Use Settings → Change hub."
            return
        }

        let projects = splitList(projectNumbersText)
        let emails = splitList(emailsText)
        let roles = splitList(roleNamesText)
        let roleIds = parseRoleIds(roleIdsText)
        let ignoredRoleNames = !roles.isEmpty && roleIds.isEmpty

        isSubmitting = true
        defer { isSubmitting = false }

        if let base = settings.baseURL {
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
            return
        }

        let tokenOk = await AutodeskTokenStore.shared.ensureValidAccessToken()
        guard tokenOk else {
            errorText = "Sign in again (Autodesk session expired)."
            NotificationCenter.default.post(name: .montySessionExpired, object: nil)
            return
        }
        guard let token = AutodeskTokenStore.shared.accessToken, !token.isEmpty else {
            errorText = "No Autodesk access token."
            return
        }

        do {
            let out = try await AccConstructionAdminClient.addUsersToProjects(
                accessToken: token,
                hubId: hubId,
                projectNumbers: projects,
                emails: emails,
                roleIds: roleIds,
                region: region.rawValue,
                dryRun: dryRun,
                ignoredRoleNamesNote: ignoredRoleNames,
            )
            resultText = out
        } catch {
            errorText = error.localizedDescription
        }
    }
}

#Preview {
    AddUsersToProjectsView()
        .environmentObject(AppSettings())
}
