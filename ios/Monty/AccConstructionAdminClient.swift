import Foundation

/// On-device ACC **Construction Admin** API (same endpoints as `aps-ai-web` `postProjectUser` / hub project listing).
/// Uses the user’s APS access token from Keychain — no Mac or Node server.
enum AccConstructionAdminError: LocalizedError {
    case noAccessToken
    case http(Int, String)

    var errorDescription: String? {
        switch self {
        case .noAccessToken:
            return "Sign in with Autodesk first."
        case .http(let code, let body):
            return "ACC API \(code): \(body)"
        }
    }
}

enum AccConstructionAdminClient {
    private static let apsBase = "https://developer.api.autodesk.com"

    struct ProjectRow {
        let projectId: String
        let projectName: String
        let projectNumber: String
    }

    static func firstProjectToken(_ name: String) -> String {
        guard let r = try? NSRegularExpression(pattern: #"^\s*([A-Za-z0-9._-]+)"#, options: []) else {
            return ""
        }
        let range = NSRange(name.startIndex..., in: name)
        guard let m = r.firstMatch(in: name, options: [], range: range),
              let tr = Range(m.range(at: 1), in: name) else { return "" }
        return String(name[tr]).trimmingCharacters(in: .whitespacesAndNewlines)
    }

    static func normalizeProjectNumber(_ value: String) -> String {
        value.trimmingCharacters(in: .whitespacesAndNewlines).uppercased()
    }

    static func normalizeEmail(_ value: String) -> String {
        value.trimmingCharacters(in: .whitespacesAndNewlines).lowercased()
    }

    /// Adds users to projects by **project number** (first token of DM project name, same heuristic as `aps-ai-web`).
    static func addUsersToProjects(
        accessToken: String,
        hubId: String,
        projectNumbers: [String],
        emails: [String],
        roleIds: [String],
        region: String,
        dryRun: Bool,
        ignoredRoleNamesNote: Bool,
    ) async throws -> String {
        let token = accessToken.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !token.isEmpty else { throw AccConstructionAdminError.noAccessToken }

        let reqNums = Array(
            Set(projectNumbers.map { normalizeProjectNumber($0) }.filter { !$0.isEmpty }),
        )
        let reqEmails = Array(Set(emails.map { normalizeEmail($0) }.filter { !$0.isEmpty }))
        guard !reqNums.isEmpty, !reqEmails.isEmpty else {
            throw AccConstructionAdminError.http(400, "projectNumbers and emails required")
        }

        let live = try await listHubProjects(accessToken: token, hubId: hubId)
        var byNumber: [String: ProjectRow] = [:]
        for p in live {
            let k = normalizeProjectNumber(p.projectNumber)
            if !k.isEmpty { byNumber[k] = p }
        }

        var matched: [ProjectRow] = []
        var missing: [String] = []
        for n in reqNums {
            if let hit = byNumber[n] {
                matched.append(hit)
            } else {
                missing.append(n)
            }
        }

        let regionHeader = region.uppercased() == "EMEA" ? "EMEA" : "US"
        let roleIdSet = Array(
            Set(roleIds.map { $0.trimmingCharacters(in: .whitespacesAndNewlines) }.filter { !$0.isEmpty }),
        )

        var resultRows: [[String: Any]] = []
        for project in matched {
            for email in reqEmails {
                if dryRun {
                    resultRows.append([
                        "projectId": project.projectId,
                        "projectName": project.projectName,
                        "projectNumber": project.projectNumber,
                        "email": email,
                        "ok": true,
                        "status": 200,
                        "response": [
                            "dry_run": true,
                            "payload_preview": [
                                "email": email,
                                "roleIds": roleIdSet,
                            ] as [String: Any],
                        ],
                    ])
                    continue
                }

                var body: [String: Any] = ["email": email]
                if !roleIdSet.isEmpty {
                    body["roleIds"] = roleIdSet
                }

                var post = try await postProjectUser(
                    accessToken: token,
                    projectId: project.projectId,
                    body: body,
                    region: regionHeader,
                )
                if !post.ok && post.status == 409 && !roleIdSet.isEmpty {
                    let users = try await listProjectUsers(
                        accessToken: token,
                        projectId: project.projectId,
                        region: regionHeader,
                    )
                    if let existing = users.first(where: { $0.email == email }) {
                        let patchBody: [String: Any] = ["roleIds": roleIdSet]
                        let patched = try await patchProjectUser(
                            accessToken: token,
                            projectId: project.projectId,
                            userId: existing.id,
                            region: regionHeader,
                            body: patchBody,
                        )
                        if patched.ok {
                            post = (true, patched.status, patched.json, nil)
                        }
                    }
                }

                var row: [String: Any] = [
                    "projectId": project.projectId,
                    "projectName": project.projectName,
                    "projectNumber": project.projectNumber,
                    "email": email,
                    "ok": post.ok,
                    "status": post.status,
                ]
                if let j = post.json {
                    row["response"] = j
                }
                if let e = post.error {
                    row["error"] = e
                }
                resultRows.append(row)
            }
        }

        let successCount = resultRows.filter { ($0["ok"] as? Bool) == true }.count
        let failureCount = resultRows.count - successCount
        let ok = missing.isEmpty && failureCount == 0 && (!matched.isEmpty || dryRun)

        var note: String?
        if ignoredRoleNamesNote {
            note =
                "Role names are not resolved on-device. Use Role IDs (comma-separated UUIDs), or leave roles empty for default membership."
        }

        let payload: [String: Any] = [
            "success": ok,
            "dry_run": dryRun,
            "matched_projects": matched.map {
                [
                    "projectId": $0.projectId,
                    "projectName": $0.projectName,
                    "projectNumber": $0.projectNumber,
                ]
            },
            "missing_project_numbers": missing,
            "requested_emails": reqEmails,
            "resolved_role_ids": roleIdSet,
            "results": resultRows,
            "note": note as Any,
        ]

        let data = try JSONSerialization.data(withJSONObject: payload, options: [.prettyPrinted, .sortedKeys])
        return String(data: data, encoding: .utf8) ?? "{}"
    }

    // MARK: - DM projects

    private static func listHubProjects(accessToken: String, hubId: String) async throws -> [ProjectRow] {
        let enc = hubId.addingPercentEncoding(withAllowedCharacters: .urlPathAllowed) ?? hubId
        var url: String? = "\(apsBase)/project/v1/hubs/\(enc)/projects"
        var rows: [ProjectRow] = []

        while let u = url {
            guard let reqUrl = URL(string: u) else { throw AccConstructionAdminError.http(400, "bad url") }
            var req = URLRequest(url: reqUrl)
            req.httpMethod = "GET"
            req.setValue("Bearer \(accessToken)", forHTTPHeaderField: "Authorization")
            req.setValue("application/json", forHTTPHeaderField: "Content-Type")

            let (data, response) = try await URLSession.shared.data(for: req)
            guard let http = response as? HTTPURLResponse else {
                throw AccConstructionAdminError.http(-1, "no response")
            }
            let text = String(data: data, encoding: .utf8) ?? ""
            guard http.statusCode == 200 else {
                throw AccConstructionAdminError.http(http.statusCode, text)
            }

            guard let obj = try JSONSerialization.jsonObject(with: data) as? [String: Any],
                  let arr = obj["data"] as? [[String: Any]] else {
                break
            }

            for item in arr {
                let pid = (item["id"] as? String)?.trimmingCharacters(in: .whitespacesAndNewlines) ?? ""
                guard !pid.isEmpty else { continue }
                let attrs = item["attributes"] as? [String: Any]
                let name = (attrs?["name"] as? String)?.trimmingCharacters(in: .whitespacesAndNewlines) ?? ""
                guard !name.isEmpty else { continue }
                let num = firstProjectToken(name)
                guard !num.isEmpty else { continue }
                rows.append(ProjectRow(projectId: pid, projectName: name, projectNumber: num))
            }

            var nextHref: String?
            if let links = obj["links"] as? [String: Any],
               let next = links["next"] as? [String: Any] {
                nextHref = (next["href"] as? String)?.trimmingCharacters(in: .whitespacesAndNewlines)
            }
            if let h = nextHref, !h.isEmpty {
                url = h.hasPrefix("http") ? h : "\(apsBase)\(h)"
            } else {
                url = nil
            }
        }

        return rows
    }

    // MARK: - Construction admin

    private static func postProjectUser(
        accessToken: String,
        projectId: String,
        body: [String: Any],
        region: String,
    ) async throws -> (ok: Bool, status: Int, json: Any?, error: String?) {
        let candidates = projectId.hasPrefix("b.") ? [projectId, String(projectId.dropFirst(2))] : [projectId]
        var lastStatus = 500
        var lastErr = ""
        for cand in candidates {
            let enc = cand.addingPercentEncoding(withAllowedCharacters: .urlPathAllowed) ?? cand
            guard let url = URL(string: "\(apsBase)/construction/admin/v1/projects/\(enc)/users") else { continue }
            var req = URLRequest(url: url)
            req.httpMethod = "POST"
            req.setValue("Bearer \(accessToken)", forHTTPHeaderField: "Authorization")
            req.setValue("application/json", forHTTPHeaderField: "Content-Type")
            req.setValue(region, forHTTPHeaderField: "Region")
            req.httpBody = try JSONSerialization.data(withJSONObject: body)

            let (data, response) = try await URLSession.shared.data(for: req)
            guard let http = response as? HTTPURLResponse else { continue }
            let text = String(data: data, encoding: .utf8) ?? ""
            let parsed: Any? = try? JSONSerialization.jsonObject(with: data)
            if http.statusCode < 300 {
                return (true, http.statusCode, parsed ?? text, nil)
            }
            lastStatus = http.statusCode
            lastErr = text
            if http.statusCode != 404 { break }
        }
        return (false, lastStatus, nil, lastErr.isEmpty ? "request failed" : lastErr)
    }

    private static func listProjectUsers(
        accessToken: String,
        projectId: String,
        region: String,
    ) async throws -> [(id: String, email: String)] {
        let candidates = projectId.hasPrefix("b.") ? [projectId, String(projectId.dropFirst(2))] : [projectId]
        for cand in candidates {
            let enc = cand.addingPercentEncoding(withAllowedCharacters: .urlPathAllowed) ?? cand
            var offset = 0
            var combined: [(id: String, email: String)] = []
            while true {
                guard let url = URL(
                    string:
                        "\(apsBase)/construction/admin/v1/projects/\(enc)/users?offset=\(offset)&limit=200",
                ) else { break }
                var req = URLRequest(url: url)
                req.httpMethod = "GET"
                req.setValue("Bearer \(accessToken)", forHTTPHeaderField: "Authorization")
                req.setValue("application/json", forHTTPHeaderField: "Content-Type")
                req.setValue(region, forHTTPHeaderField: "Region")

                let (data, response) = try await URLSession.shared.data(for: req)
                guard let http = response as? HTTPURLResponse, http.statusCode == 200 else { break }
                guard let obj = try JSONSerialization.jsonObject(with: data) as? [String: Any],
                      let results = obj["results"] as? [[String: Any]] else { break }
                for row in results {
                    let id = (row["id"] as? String)?.trimmingCharacters(in: .whitespacesAndNewlines) ?? ""
                    let email = (row["email"] as? String)?.trimmingCharacters(in: .whitespacesAndNewlines).lowercased()
                        ?? ""
                    if !id.isEmpty, !email.isEmpty {
                        combined.append((id, email))
                    }
                }
                if results.count < 200 { break }
                offset += results.count
            }
            if !combined.isEmpty { return combined }
        }
        return []
    }

    private static func patchProjectUser(
        accessToken: String,
        projectId: String,
        userId: String,
        region: String,
        body: [String: Any],
    ) async throws -> (ok: Bool, status: Int, json: Any?) {
        let candidates = projectId.hasPrefix("b.") ? [projectId, String(projectId.dropFirst(2))] : [projectId]
        for cand in candidates {
            let pe = cand.addingPercentEncoding(withAllowedCharacters: .urlPathAllowed) ?? cand
            let ue = userId.addingPercentEncoding(withAllowedCharacters: .urlPathAllowed) ?? userId
            guard let url = URL(
                string: "\(apsBase)/construction/admin/v1/projects/\(pe)/users/\(ue)",
            ) else { continue }
            var req = URLRequest(url: url)
            req.httpMethod = "PATCH"
            req.setValue("Bearer \(accessToken)", forHTTPHeaderField: "Authorization")
            req.setValue("application/json", forHTTPHeaderField: "Content-Type")
            req.setValue(region, forHTTPHeaderField: "Region")
            req.httpBody = try JSONSerialization.data(withJSONObject: body)

            let (data, response) = try await URLSession.shared.data(for: req)
            guard let http = response as? HTTPURLResponse else { continue }
            let parsed: Any? = try? JSONSerialization.jsonObject(with: data)
            if http.statusCode < 300 {
                return (true, http.statusCode, parsed)
            }
            if http.statusCode != 404 { break }
        }
        return (false, 500, nil)
    }
}
