#nullable disable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using DesignAutomationFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Metromont.RevitMarkWorkitem;

/// <summary>Environment names used by the Design Automation execution entrypoint.</summary>
public static class DaRuntimeEnv
{
    internal const string EnvPayload = "MARK_PAYLOAD_JSON";
    internal const string EnvAuditLegacy = "MARK_AUDIT_JSON";
    /// <summary>Preferred path for the audit artifact (Design Automation can zip this folder).</summary>
    internal const string EnvAuditReport = "DA_AUDIT_REPORT_JSON";
    internal const string EnvArtifactsDir = "DA_ARTIFACTS_DIR";
    internal const string EnvSwc = "MARK_SWC";
}

/// <summary>
/// Design Automation entrypoint. Reuses the same dispatcher as desktop command mode.
/// </summary>
public class DesignAutomationEntryPoint : IExternalDBApplication
{
    public ExternalDBApplicationResult OnStartup(ControlledApplication application)
    {
        DesignAutomationBridge.DesignAutomationReadyEvent += OnDesignAutomationReady;
        return ExternalDBApplicationResult.Succeeded;
    }

    public ExternalDBApplicationResult OnShutdown(ControlledApplication application)
    {
        DesignAutomationBridge.DesignAutomationReadyEvent -= OnDesignAutomationReady;
        return ExternalDBApplicationResult.Succeeded;
    }

    private static void OnDesignAutomationReady(object sender, DesignAutomationReadyEventArgs e)
    {
        var message = string.Empty;
        try
        {
            Console.WriteLine("[DA] DesignAutomationReady event received.");
            var result = RevitAutomationDispatcher.Run(e.DesignAutomationData, ref message);
            e.Succeeded = result;
            Console.WriteLine($"[DA] Run result={result}; message={message}");
        }
        catch (Exception ex)
        {
            e.Succeeded = false;
            Console.WriteLine($"[DA] Unhandled exception: {ex}");
        }
    }
}

/// <summary>Core dispatcher; add new operations here over time.</summary>
internal static class RevitAutomationDispatcher
{
    public static bool Run(DesignAutomationData daData, ref string message)
    {
        if (!TryLoadPayload(out var root, out var payloadErr))
        {
            message = payloadErr;
            WriteMinimalFailureAudit(payloadErr);
            return false;
        }

        ExpandCachedSelectionUpdates(root);

        var doc = daData?.RevitDoc;
        var openedCloudDoc = false;
        if (TryResolveCloudModelTarget(root, out var cloudRegion, out var projectGuid, out var modelGuid))
        {
            if (daData?.RevitApp == null)
            {
                message = "RCW target requested, but Revit application context is unavailable.";
                WriteMinimalFailureAudit(message);
                return false;
            }

            try
            {
                var cloudPath = ModelPathUtils.ConvertCloudGUIDsToCloudPath(cloudRegion, projectGuid, modelGuid);
                doc = daData.RevitApp.OpenDocumentFile(cloudPath, new OpenOptions());
                openedCloudDoc = doc != null;
            }
            catch (Exception ex)
            {
                message = $"Failed to open Revit cloud model ({cloudRegion}/{projectGuid}/{modelGuid}): {ex.Message}";
                WriteMinimalFailureAudit(message);
                return false;
            }
        }

        try
        {
            return Run(doc, root, ref message);
        }
        finally
        {
            if (openedCloudDoc && doc != null)
            {
                try
                {
                    doc.Close(false);
                }
                catch
                {
                    /* best effort in DA sandbox */
                }
            }
        }
    }

    private static bool Run(Document doc, JObject root, ref string message)
    {
        if (doc == null)
        {
            message = "No active document.";
            WriteMinimalFailureAudit("No active document — cannot run Design Automation on a closed or missing model.");
            return false;
        }

        var audit = NewAuditRoot(doc, root);
        var log = (JArray)audit["log"];

        void Log(string level, string text)
        {
            log.Add(new JObject
            {
                ["level"] = level,
                ["text"] = text,
                ["time_utc"] = DateTime.UtcNow.ToString("o"),
            });
        }

        Log("info", $"Document: {doc.Title}; operation (raw): {root["operation"]?.ToString() ?? "(infer)"}");

        ValidateCachedSelectionForExecution(root, audit, Log);
        ValidateEditTarget(doc, root, audit, Log);

        var marks = root["marks"] as JArray ?? new JArray();
        var sharedGuidMap = root["sharedParameterGuidMap"] as JObject;
        var skipAnalysis = root["skip_analysis"]?.Value<bool>() == true;
        var op = root["operation"]?.ToString()?.Trim() ?? "";

        if (string.Equals(op, "clear_cache", StringComparison.OrdinalIgnoreCase))
        {
            audit["result"] = "clear_cache_no_op";
            audit["note"] = "No model changes; placeholder for future cache invalidation.";
            Log("info", audit["note"].ToString());
            PostRunVerify(doc, audit, Log, transactionCommitted: false, swcAttempted: false);
            StampAuditOutcome(audit, success: true, error: null);
            WriteAuditArtifacts(audit);
            return true;
        }

        if (string.IsNullOrEmpty(op))
        {
            if (skipAnalysis)
                op = "modify_parameters";
            else if (marks.Count > 0)
                op = "apply_marks_and_modify";
            else
                op = "modify_parameters";
        }

        if (skipAnalysis)
            op = "modify_parameters";

        var runMarks = !skipAnalysis && IsMarkOperation(op);
        var runModify = IsModifyOperation(op);

        audit["resolved_operation"] = op;
        audit["run_marks"] = runMarks;
        audit["run_modify"] = runModify;
        Log("info", $"Resolved operation={op}; run_marks={runMarks}; run_modify={runModify}; skip_analysis={skipAnalysis}");

        if (!runMarks && !runModify)
        {
            audit["result"] = "no_op";
            var warn = $"Unknown or unsupported operation after resolution: {op}";
            audit["warning"] = warn;
            Log("error", warn);
            PostRunVerify(doc, audit, Log, transactionCommitted: false, swcAttempted: false);
            StampAuditOutcome(audit, success: false, error: warn);
            WriteAuditArtifacts(audit);
            message = warn;
            return false;
        }

        var txLabel = runMarks && runModify
            ? "DA: marks + parameters"
            : runMarks
                ? "DA: run_mark_analysis"
                : "DA: modify_parameters";
        using var tx = new Transaction(doc, txLabel);
        tx.Start();
        try
        {
            if (runMarks)
                RunMarkAnalysisPass(doc, marks, sharedGuidMap, audit, Log);

            if (runModify)
            {
                ApplyParameterUpdates(doc, root, sharedGuidMap, audit, Log);
                ApplyParameterPatches(doc, root, sharedGuidMap, audit, Log);
            }

            tx.Commit();
        }
        catch (Exception ex)
        {
            tx.RollBack();
            audit["transaction"] = "rolled_back";
            audit["error"] = $"Transaction failed and was rolled back: {ex.Message}";
            Log("error", $"Transaction rolled back: {ex.Message}");
            PostRunVerify(doc, audit, Log, transactionCommitted: false, swcAttempted: false);
            StampAuditOutcome(audit, success: false, error: audit["error"]?.ToString());
            WriteAuditArtifacts(audit);
            message = ex.Message;
            return false;
        }

        audit["transaction"] = "committed";
        Log("info", "Transaction committed successfully.");
        var swcAttempted = TrySynchronizeWithCentral(doc, audit, Log);
        PostRunVerify(doc, audit, Log, transactionCommitted: true, swcAttempted);
        var (bizOk, bizErr) = ComputeBusinessOutcome(audit, runModify, transactionCommitted: true);
        StampAuditOutcome(audit, bizOk, bizErr);
        WriteAuditArtifacts(audit);

        message = SummarizeForHost(audit);
        if (!bizOk)
        {
            if (!string.IsNullOrWhiteSpace(bizErr))
                message = $"{message} business_error={bizErr}";
            return false;
        }
        return true;
    }

    private static string ResolvePayloadPath()
    {
        var fromEnv = Environment.GetEnvironmentVariable(DaRuntimeEnv.EnvPayload)?.Trim();
        if (!string.IsNullOrWhiteSpace(fromEnv) && !fromEnv.Contains("$("))
            return fromEnv;

        var cwdCandidate = Path.Combine(Environment.CurrentDirectory, "mark-payload.json");
        if (File.Exists(cwdCandidate))
            return cwdCandidate;

        return Path.Combine(Path.GetTempPath(), "mark-payload.json");
    }

    private static bool TryLoadPayload(out JObject root, out string error)
    {
        root = null;
        error = "";
        var payloadPath = ResolvePayloadPath();
        if (!File.Exists(payloadPath))
        {
            error = $"Payload not found: {payloadPath}";
            return false;
        }

        try
        {
            var json = File.ReadAllText(payloadPath);
            root = JObject.Parse(json);
            return true;
        }
        catch (Exception ex)
        {
            error = $"Invalid payload JSON: {ex.Message}";
            return false;
        }
    }

    private static bool TryResolveCloudModelTarget(
        JObject root,
        out string region,
        out Guid projectGuid,
        out Guid modelGuid)
    {
        region = "";
        projectGuid = Guid.Empty;
        modelGuid = Guid.Empty;
        var cm = root?["cloud_model"] as JObject;
        if (cm == null) return false;
        var regionRaw = cm["region"]?.ToString()?.Trim();
        var projectRaw = cm["projectGuid"]?.ToString()?.Trim();
        var modelRaw = cm["modelGuid"]?.ToString()?.Trim();
        if (string.IsNullOrWhiteSpace(regionRaw) ||
            string.IsNullOrWhiteSpace(projectRaw) ||
            string.IsNullOrWhiteSpace(modelRaw))
            return false;
        if (!Guid.TryParse(projectRaw, out projectGuid) || !Guid.TryParse(modelRaw, out modelGuid))
            return false;
        var upper = regionRaw.ToUpperInvariant();
        region = upper.Contains("EMEA") ? "EMEA" : "US";
        return true;
    }

    /// <summary>Top-level outcome for web / DA consumers; always written to audit_report.json.</summary>
    private static void StampAuditOutcome(JObject audit, bool success, string error)
    {
        audit["success"] = success;
        audit["error"] = string.IsNullOrWhiteSpace(error) ? null : error;
    }

    private static void WriteMinimalFailureAudit(string error)
    {
        var audit = new JObject
        {
            ["schema"] = "revit_automation_audit_v2",
            ["timestamp_utc"] = DateTime.UtcNow.ToString("o"),
            ["success"] = false,
            ["error"] = error,
            ["log"] = new JArray
            {
                new JObject
                {
                    ["level"] = "error",
                    ["text"] = error,
                    ["time_utc"] = DateTime.UtcNow.ToString("o"),
                },
            },
            ["summary"] = new JObject
            {
                ["unique_elements_resolved_modify"] = 0,
                ["parameter_actions_ok"] = 0,
                ["parameter_actions_failed"] = 0,
            },
            ["modify"] = new JObject
            {
                ["parameter_actions_ok"] = 0,
                ["parameter_actions_failed"] = 0,
                ["external_id_attempts"] = 0,
                ["elements_matched"] = 0,
                ["elements_missed"] = 0,
            },
            ["post_run"] = new JObject { ["summary_line"] = error },
        };
        WriteAuditArtifacts(audit);
    }

    /// <summary>False when SWC failed, no elements resolved despite attempts, or all modify actions failed.</summary>
    private static (bool ok, string err) ComputeBusinessOutcome(
        JObject audit,
        bool runModify,
        bool transactionCommitted)
    {
        if (!transactionCommitted)
            return (false, audit["error"]?.ToString() ?? "Transaction not committed.");

        var swc = audit["swc"] as JObject;
        var swcSt = swc?["status"]?.ToString()?.ToLowerInvariant() ?? "";
        if (swcSt == "failed")
            return (false, "SyncWithCentral failed with errors: " + (swc?["error"]?.ToString() ?? "unknown"));

        if (!runModify)
            return (true, null);

        var mod = audit["modify"] as JObject;
        var attempts = mod?["external_id_attempts"]?.Value<int>() ?? 0;
        var okc = mod?["parameter_actions_ok"]?.Value<int>() ?? 0;
        var failc = mod?["parameter_actions_failed"]?.Value<int>() ?? 0;
        var uniq = audit["summary"]?["unique_elements_resolved_modify"]?.Value<int>() ?? 0;

        if (attempts > 0 && uniq == 0 && okc == 0)
            return (false, "No elements resolved from cached externalIds; no parameter changes applied.");

        if (uniq > 0 && okc == 0 && failc > 0)
            return (false, "All parameter actions failed (read-only, not found, or exception). See modify.failures in audit_report.json.");

        return (true, null);
    }

    private static JObject NewAuditRoot(Document doc, JObject root)
    {
        return new JObject
        {
            ["schema"] = "revit_automation_audit_v2",
            ["timestamp_utc"] = DateTime.UtcNow.ToString("o"),
            ["document_title"] = doc.Title,
            ["document_path"] = doc.PathName,
            ["is_workshared"] = doc.IsWorkshared,
            ["payload_operation"] = root["operation"]?.ToString(),
            ["skip_analysis"] = root["skip_analysis"]?.Value<bool>() ?? false,
            ["cache_id"] = root["cache_id"]?.ToString()
                           ?? root["cached_selection"]?["cache_id"]?.ToString(),
            ["log"] = new JArray(),
            ["validation"] = new JObject
            {
                ["cached_selection_warnings"] = new JArray(),
                ["edit_target_warnings"] = new JArray(),
            },
            ["summary"] = new JObject
            {
                ["unique_elements_resolved_modify"] = 0,
                ["parameter_actions_ok"] = 0,
                ["parameter_actions_failed"] = 0,
            },
            ["marks"] = new JObject
            {
                ["groups_processed"] = 0,
                ["external_id_attempts"] = 0,
                ["elements_matched"] = 0,
                ["elements_missed"] = 0,
                ["parameters_set"] = 0,
                ["parameters_skipped_readonly"] = 0,
                ["misses"] = new JArray(),
            },
            ["modify"] = new JObject
            {
                ["parameter_update_rows"] = 0,
                ["external_id_attempts"] = 0,
                ["elements_matched"] = 0,
                ["elements_missed"] = 0,
                ["parameter_actions_ok"] = 0,
                ["parameter_actions_failed"] = 0,
                ["misses"] = new JArray(),
                ["failures"] = new JArray(),
            },
            ["patches"] = new JObject
            {
                ["rows"] = 0,
                ["external_id_attempts"] = 0,
                ["elements_matched"] = 0,
                ["elements_missed"] = 0,
                ["parameters_set"] = 0,
                ["parameters_skipped"] = 0,
            },
            ["swc"] = new JObject { ["attempted"] = false, ["status"] = "skipped" },
            ["post_run"] = new JObject(),
        };
    }

    /// <summary>
    /// Writes audit JSON for Design Automation packaging: canonical <c>audit_report.json</c> plus legacy path.
    /// Set <c>DA_ARTIFACTS_DIR</c> to a folder DA zips as workitem output; override file with <c>DA_AUDIT_REPORT_JSON</c>.
    /// </summary>
    private static void WriteAuditArtifacts(JObject audit)
    {
        var dir = Environment.GetEnvironmentVariable(DaRuntimeEnv.EnvArtifactsDir);
        var defaultReport = string.IsNullOrWhiteSpace(dir)
            ? Path.Combine(Path.GetTempPath(), "audit_report.json")
            : Path.Combine(dir.Trim(), "audit_report.json");
        var reportPath = Environment.GetEnvironmentVariable(DaRuntimeEnv.EnvAuditReport);
        if (string.IsNullOrWhiteSpace(reportPath))
            reportPath = defaultReport;
        else
            reportPath = reportPath.Trim();

        var legacyPath = Environment.GetEnvironmentVariable(DaRuntimeEnv.EnvAuditLegacy);
        if (string.IsNullOrWhiteSpace(legacyPath))
            legacyPath = Path.Combine(Path.GetTempPath(), "mark-audit.json");

        audit["artifact_paths"] = new JObject
        {
            ["audit_report_json"] = reportPath,
            ["mark_audit_json_legacy"] = legacyPath,
        };

        var text = audit.ToString(Formatting.Indented);
        TryWrite(reportPath, text);
        TryWrite(legacyPath, text);
    }

    private static void TryWrite(string path, string text)
    {
        try
        {
            var folder = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            File.WriteAllText(path, text);
        }
        catch
        {
            /* DA sandbox — best effort */
        }
    }

    private static void ExpandCachedSelectionUpdates(JObject root)
    {
        var sel = root["cached_selection"] as JObject;
        var updates = root["updates"] as JArray;
        if (sel == null || updates == null || updates.Count == 0)
            return;

        var extIds = sel["externalIds"]?.ToObject<List<string>>() ?? new List<string>();
        var cleaned = new List<string>();
        foreach (var x in extIds)
        {
            if (!string.IsNullOrWhiteSpace(x))
                cleaned.Add(x.Trim());
        }

        if (cleaned.Count == 0)
            return;

        var paramUpdates = root["parameter_updates"] as JArray ?? new JArray();
        root["parameter_updates"] = paramUpdates;

        foreach (var u in updates)
        {
            if (u is not JObject row) continue;
            var paramName = NormalizeIncomingParamName(row["paramName"]?.ToString());
            var action = row["action"]?.ToString()?.Trim()?.ToLowerInvariant();
            if (string.IsNullOrEmpty(paramName) || string.IsNullOrEmpty(action)) continue;
            if (action != "clear" && action != "set" && action != "toggle") continue;

            var expanded = new JObject
            {
                ["externalIds"] = JArray.FromObject(cleaned),
                ["paramName"] = paramName,
                ["action"] = action,
            };
            if (row["value"] != null && row["value"].Type != JTokenType.Null)
                expanded["value"] = row["value"];
            paramUpdates.Add(expanded);
        }
    }

    private static void ValidateCachedSelectionForExecution(JObject root, JObject audit, Action<string, string> log)
    {
        var arr = (JArray)audit["validation"]!["cached_selection_warnings"]!;
        var hasUpdates = root["updates"] is JArray ua && ua.Count > 0;
        var hasParamUpdates = root["parameter_updates"] is JArray pa && pa.Count > 0;
        var sel = root["cached_selection"] as JObject;

        if ((hasUpdates || hasParamUpdates) && sel == null && hasUpdates)
        {
            var w = "cached_selection is missing but updates[] is present; expansion skipped — provide cached_selection.externalIds.";
            arr.Add(w);
            log("warning", w);
            return;
        }

        if (sel == null)
            return;

        var ext = sel["externalIds"] as JArray;
        if (ext == null || ext.Count == 0)
        {
            var w = "cached_selection.externalIds is empty or missing.";
            arr.Add(w);
            log("warning", w);
        }

        var prov = sel["provenance"] as JObject;
        if (prov == null || prov["analyzedAt"] == null)
        {
            var w = "cached_selection.provenance.analyzedAt not set — cannot verify staleness (stub for validate_edit_target).";
            arr.Add(w);
            log("warning", w);
        }
    }

    /// <summary>MCP Tool B placeholder: compare published-version metadata to the opened central model.</summary>
    private static void ValidateEditTarget(Document doc, JObject root, JObject audit, Action<string, string> log)
    {
        var arr = (JArray)audit["validation"]!["edit_target_warnings"]!;
        _ = doc;
        _ = root;
        var w =
            "validate_edit_target (Tool B): stub — future compare of payload provenance / published version vs opened central model.";
        arr.Add(w);
        log("info", w);
    }

    /// <summary>MCP Tool D placeholder: final narrative for chat / downstream validators.</summary>
    private static void PostRunVerify(
        Document doc,
        JObject audit,
        Action<string, string> log,
        bool transactionCommitted,
        bool swcAttempted)
    {
        var post = (JObject)audit["post_run"]!;
        post["transaction_committed"] = transactionCommitted;
        post["swc_attempted"] = swcAttempted;
        post["swc_status"] = audit["swc"]?["status"]?.ToString() ?? "n/a";

        var mod = audit["modify"] as JObject;
        var ok = mod?["parameter_actions_ok"]?.Value<int>() ?? 0;
        var fail = mod?["parameter_actions_failed"]?.Value<int>() ?? 0;
        post["modify_parameter_ok"] = ok;
        post["modify_parameter_failed"] = fail;

        var summary = (JObject)audit["summary"]!;
        summary["parameter_actions_ok"] = ok;
        summary["parameter_actions_failed"] = fail;

        var line = transactionCommitted
            ? $"Post-run: committed; parameter actions ok={ok}, failed={fail}; SWC={(audit["swc"]?["status"] ?? "n/a")}."
            : "Post-run: transaction not committed.";
        post["summary_line"] = line;
        log("info", line);
    }

    private static string SummarizeForHost(JObject audit)
    {
        var op = audit["resolved_operation"]?.ToString() ?? "";
        var mod = audit["modify"] as JObject;
        var ok = mod?["parameter_actions_ok"]?.Value<int>() ?? 0;
        var fail = mod?["parameter_actions_failed"]?.Value<int>() ?? 0;
        var swc = audit["swc"] as JObject;
        var swcStatus = swc?["status"]?.ToString() ?? "unknown";
        var swcError = swc?["error"]?.ToString() ?? "";
        var swcPart = string.IsNullOrWhiteSpace(swcError)
            ? $"swc={swcStatus}"
            : $"swc={swcStatus}; swc_error={swcError}";
        return $"run_revit_automation complete: operation={op}; modify_ok={ok}; modify_failed={fail}; {swcPart}. See audit_report.json.";
    }

    private static bool IsMarkOperation(string op) =>
        string.Equals(op, "apply_marks", StringComparison.OrdinalIgnoreCase)
        || string.Equals(op, "apply_marks_and_modify", StringComparison.OrdinalIgnoreCase)
        || string.Equals(op, "run_mark_analysis", StringComparison.OrdinalIgnoreCase);

    private static bool IsModifyOperation(string op) =>
        string.Equals(op, "modify_parameters", StringComparison.OrdinalIgnoreCase)
        || string.Equals(op, "apply_marks_and_modify", StringComparison.OrdinalIgnoreCase);

    private static void RunMarkAnalysisPass(
        Document doc,
        JArray marks,
        JObject sharedGuidMap,
        JObject audit,
        Action<string, string> log)
    {
        var sec = (JObject)audit["marks"]!;
        foreach (var m in marks)
        {
            var mark = m?["control_mark"]?.ToString();
            var extIds = m?["externalIds"]?.ToObject<List<string>>() ?? new List<string>();
            if (string.IsNullOrEmpty(mark)) continue;
            sec["groups_processed"] = sec["groups_processed"]!.Value<int>() + 1;

            foreach (var ext in extIds)
            {
                if (string.IsNullOrWhiteSpace(ext)) continue;
                sec["external_id_attempts"] = sec["external_id_attempts"]!.Value<int>() + 1;
                var target = FindByExternalId(doc, ext);
                if (target == null)
                {
                    sec["elements_missed"] = sec["elements_missed"]!.Value<int>() + 1;
                    AppendMiss(sec, ext, "CONTROL_MARK", "element_not_found");
                    log("warning", $"Mark pass: no element for External ID '{ext}'.");
                    continue;
                }

                sec["elements_matched"] = sec["elements_matched"]!.Value<int>() + 1;
                var p = ResolveParameter(target, "CONTROL_MARK", sharedGuidMap);
                if (p == null)
                {
                    AppendMiss(sec, ext, "CONTROL_MARK", "parameter_not_found");
                    log("warning", $"Mark pass: CONTROL_MARK not found on element {target.Id} (externalId={ext}).");
                    continue;
                }

                if (p.IsReadOnly)
                {
                    sec["parameters_skipped_readonly"] = sec["parameters_skipped_readonly"]!.Value<int>() + 1;
                    AppendMiss(sec, ext, "CONTROL_MARK", "read_only");
                    log("warning", $"Mark pass: CONTROL_MARK read-only on element {target.Id}.");
                    continue;
                }

                try
                {
                    p.Set(mark);
                    sec["parameters_set"] = sec["parameters_set"]!.Value<int>() + 1;
                    log("info", $"Mark pass: set CONTROL_MARK={mark} on element {target.Id}.");
                }
                catch (Exception ex)
                {
                    sec["elements_missed"] = sec["elements_missed"]!.Value<int>() + 1;
                    log("warning", $"Mark pass: failed to set CONTROL_MARK on {target.Id}: {ex.Message}");
                }
            }
        }
    }

    private static void AppendMiss(JObject section, string externalId, string paramName, string reason)
    {
        var arr = section["misses"] as JArray ?? new JArray();
        section["misses"] = arr;
        arr.Add(new JObject
        {
            ["externalId"] = externalId,
            ["paramName"] = paramName,
            ["reason"] = reason,
        });
    }

    private static void AppendFailure(JObject section, string externalId, string paramName, string action, string reason)
    {
        var arr = section["failures"] as JArray ?? new JArray();
        section["failures"] = arr;
        arr.Add(new JObject
        {
            ["externalId"] = externalId,
            ["paramName"] = paramName,
            ["action"] = action,
            ["reason"] = reason,
        });
    }

    private static void ApplyParameterUpdates(
        Document doc,
        JObject root,
        JObject sharedGuidMap,
        JObject audit,
        Action<string, string> log)
    {
        var sec = (JObject)audit["modify"]!;
        var uniqueResolved = new HashSet<long>();
        if (root["parameter_updates"] is not JArray arr)
            return;

        foreach (var item in arr)
        {
            sec["parameter_update_rows"] = sec["parameter_update_rows"]!.Value<int>() + 1;
            if (item is not JObject row) continue;
            var extIds = row["externalIds"]?.ToObject<List<string>>() ?? new List<string>();
            var paramName = row["paramName"]?.ToString()?.Trim();
            var action = row["action"]?.ToString()?.Trim()?.ToLowerInvariant();
            if (string.IsNullOrEmpty(paramName) || string.IsNullOrEmpty(action)) continue;

            foreach (var ext in extIds)
            {
                if (string.IsNullOrWhiteSpace(ext)) continue;
                sec["external_id_attempts"] = sec["external_id_attempts"]!.Value<int>() + 1;
                var target = FindByExternalId(doc, ext);
                if (target == null)
                {
                    sec["elements_missed"] = sec["elements_missed"]!.Value<int>() + 1;
                    AppendMiss(sec, ext, paramName, "element_not_found");
                    log("warning", $"Modify: element not found for External ID '{ext}' ({paramName}, {action}).");
                    continue;
                }

                uniqueResolved.Add(target.Id.Value);
                sec["elements_matched"] = sec["elements_matched"]!.Value<int>() + 1;
                var p = ResolveParameter(target, paramName, sharedGuidMap);
                if (p == null)
                {
                    sec["parameter_actions_failed"] = sec["parameter_actions_failed"]!.Value<int>() + 1;
                    AppendFailure(sec, ext, paramName, action, "parameter_not_found");
                    log("warning", $"Modify: parameter '{paramName}' not found on element {target.Id}.");
                    continue;
                }

                if (p.IsReadOnly)
                {
                    sec["parameter_actions_failed"] = sec["parameter_actions_failed"]!.Value<int>() + 1;
                    AppendFailure(sec, ext, paramName, action, "read_only");
                    log("warning", $"Modify: '{paramName}' is read-only on element {target.Id}.");
                    continue;
                }

                try
                {
                    if (action == "clear")
                        ApplyClearParameter(p, log, target, paramName);
                    else if (action == "set")
                        SetParameterFromToken(p, row["value"], log, target, paramName);
                    else if (action == "toggle")
                        ToggleParameterValue(p, log, target, paramName);
                    else
                    {
                        sec["parameter_actions_failed"] = sec["parameter_actions_failed"]!.Value<int>() + 1;
                        AppendFailure(sec, ext, paramName, action, "unknown_action");
                        log("warning", $"Modify: unknown action '{action}' for {paramName} on {target.Id}.");
                        continue;
                    }

                    sec["parameter_actions_ok"] = sec["parameter_actions_ok"]!.Value<int>() + 1;
                    log("info", $"Modify: {action} '{paramName}' on element {target.Id} (externalId={ext}).");
                }
                catch (Exception ex)
                {
                    sec["parameter_actions_failed"] = sec["parameter_actions_failed"]!.Value<int>() + 1;
                    AppendFailure(sec, ext, paramName, action, "exception:" + ex.Message);
                    log("warning", $"Modify: exception on {paramName} / {target.Id}: {ex.Message}");
                }
            }
        }

        ((JObject)audit["summary"]!)["unique_elements_resolved_modify"] = uniqueResolved.Count;
    }

    private static string NormalizeIncomingParamName(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return "";
        var up = raw.Trim().ToUpperInvariant().Replace(" ", "_");
        if (up == "MARK" || up == "MARKS" || up == "PIECE_MARK" || up == "PIECE_MARKS" || up == "CONTROL_MARKS")
            return "CONTROL_MARK";
        if (up == "CONTROL_NUMBERS")
            return "CONTROL_NUMBER";
        return up;
    }

    /// <summary>Prefer API <see cref="Parameter.ClearValue"/>; fall back to type-specific clears.</summary>
    private static void ApplyClearParameter(Parameter p, Action<string, string> log, Element target, string paramName)
    {
        try
        {
            if (p.ClearValue())
                return;
        }
        catch (Exception ex)
        {
            log("info", $"ClearValue() not used for {paramName} on {target.Id}: {ex.Message}");
        }

        ClearParameterValueFallback(p);
    }

    private static void ClearParameterValueFallback(Parameter p)
    {
        switch (p.StorageType)
        {
            case StorageType.String:
                p.Set(string.Empty);
                break;
            case StorageType.Double:
                p.Set(0.0);
                break;
            case StorageType.Integer:
                p.Set(0);
                break;
            case StorageType.ElementId:
                p.Set(ElementId.InvalidElementId);
                break;
            default:
                break;
        }
    }

    private static void ToggleParameterValue(Parameter p, Action<string, string> log, Element target, string paramName)
    {
        if (p.StorageType == StorageType.Integer)
        {
            var v = p.AsInteger();
            p.Set(v == 0 ? 1 : 0);
            return;
        }

        if (p.StorageType == StorageType.String)
        {
            var s = p.AsString() ?? "";
            if (string.Equals(s, "Yes", StringComparison.OrdinalIgnoreCase))
                p.Set("No");
            else if (string.Equals(s, "No", StringComparison.OrdinalIgnoreCase))
                p.Set("Yes");
            return;
        }

        throw new InvalidOperationException(
            $"Toggle unsupported for storage type {p.StorageType} on '{paramName}' (element {target.Id}).");
    }

    private static void SetParameterFromToken(
        Parameter p,
        JToken valueToken,
        Action<string, string> log,
        Element target,
        string paramName)
    {
        switch (p.StorageType)
        {
            case StorageType.String:
                var s = valueToken == null || valueToken.Type == JTokenType.Null ? "" : valueToken.ToString();
                p.Set(s ?? "");
                break;
            case StorageType.Double:
                if (valueToken != null && (valueToken.Type == JTokenType.Float || valueToken.Type == JTokenType.Integer))
                    p.Set(valueToken.Value<double>());
                else if (valueToken != null && double.TryParse(valueToken.ToString(), System.Globalization.NumberStyles.Any,
                             System.Globalization.CultureInfo.InvariantCulture, out var d))
                    p.Set(d);
                else
                    throw new InvalidOperationException($"Could not parse double for '{paramName}' on element {target.Id}.");
                break;
            case StorageType.Integer:
                if (valueToken != null && (valueToken.Type == JTokenType.Integer || valueToken.Type == JTokenType.Float))
                    p.Set(valueToken.Value<int>());
                else if (valueToken != null && int.TryParse(valueToken.ToString(), out var i))
                    p.Set(i);
                else
                    throw new InvalidOperationException($"Could not parse integer for '{paramName}' on element {target.Id}.");
                break;
            case StorageType.ElementId:
                if (valueToken != null && valueToken.Type == JTokenType.Integer)
                    p.Set(new ElementId(valueToken.Value<long>()));
                else
                    throw new InvalidOperationException($"ElementId value required for '{paramName}' on element {target.Id}.");
                break;
            default:
                throw new InvalidOperationException(
                    $"Set unsupported for storage type {p.StorageType} on '{paramName}' (element {target.Id}).");
        }
    }

    private static void ApplyParameterPatches(
        Document doc,
        JObject root,
        JObject sharedGuidMap,
        JObject audit,
        Action<string, string> log)
    {
        var sec = (JObject)audit["patches"]!;
        var combined = new List<JToken>();
        if (root["parameterPatches"] is JArray top)
            foreach (var x in top)
                combined.Add(x);
        var nested = root["additional_updates"]?["parameterPatches"] as JArray;
        if (nested != null)
            foreach (var x in nested)
                combined.Add(x);

        foreach (var item in combined)
        {
            sec["rows"] = sec["rows"]!.Value<int>() + 1;
            if (item is not JObject row) continue;
            var extIds = row["externalIds"]?.ToObject<List<string>>() ?? new List<string>();
            var setObj = row["set"] as JObject;
            if (setObj == null || setObj.Count == 0) continue;

            foreach (var ext in extIds)
            {
                if (string.IsNullOrWhiteSpace(ext)) continue;
                sec["external_id_attempts"] = sec["external_id_attempts"]!.Value<int>() + 1;
                var target = FindByExternalId(doc, ext);
                if (target == null)
                {
                    sec["elements_missed"] = sec["elements_missed"]!.Value<int>() + 1;
                    log("warning", $"Patch: element not found for External ID '{ext}'.");
                    continue;
                }

                sec["elements_matched"] = sec["elements_matched"]!.Value<int>() + 1;
                foreach (var prop in setObj.Properties())
                {
                    var p = ResolveParameter(target, prop.Name, sharedGuidMap);
                    if (p == null)
                    {
                        sec["parameters_skipped"] = sec["parameters_skipped"]!.Value<int>() + 1;
                        continue;
                    }

                    if (p.IsReadOnly)
                    {
                        sec["parameters_skipped"] = sec["parameters_skipped"]!.Value<int>() + 1;
                        continue;
                    }

                    try
                    {
                        SetParameterFromToken(p, prop.Value, log, target, prop.Name);
                        sec["parameters_set"] = sec["parameters_set"]!.Value<int>() + 1;
                        log("info", $"Patch: set '{prop.Name}' on element {target.Id}.");
                    }
                    catch (Exception ex)
                    {
                        sec["parameters_skipped"] = sec["parameters_skipped"]!.Value<int>() + 1;
                        log("warning", $"Patch: failed '{prop.Name}' on {target.Id}: {ex.Message}");
                    }
                }
            }
        }
    }

    private static bool TrySynchronizeWithCentral(Document doc, JObject audit, Action<string, string> log)
    {
        var swc = (JObject)audit["swc"]!;
        var flag = Environment.GetEnvironmentVariable(DaRuntimeEnv.EnvSwc);
        if (!string.Equals(flag, "true", StringComparison.OrdinalIgnoreCase))
        {
            swc["status"] = "skipped_not_enabled";
            swc["hint"] = $"Set {DaRuntimeEnv.EnvSwc}=true to attempt SynchronizeWithCentral after commit.";
            log("info", "SWC skipped (MARK_SWC not true).");
            return false;
        }

        if (!doc.IsWorkshared)
        {
            swc["attempted"] = true;
            swc["status"] = "skipped_not_workshared";
            log("info", "SWC skipped (document not workshared).");
            return true;
        }

        try
        {
            swc["attempted"] = true;
            // In DA cloud-opened docs, STC can fail with "Without Save" if Revit expects
            // an explicit save point before synchronization.
            TrySaveBeforeSwc(doc, swc, log, phase: "before_sync");
            var twc = new TransactWithCentralOptions();
            var opts = new SynchronizeWithCentralOptions
            {
                Comment = "Design Automation — run_revit_automation",
                SaveLocalBefore = true,
                SaveLocalAfter = true,
            };
            doc.SynchronizeWithCentral(twc, opts);
            swc["status"] = "ok";
            log("info", "SynchronizeWithCentral completed.");
            return true;
        }
        catch (Exception ex)
        {
            // Recover common cloud-worksharing failure once by adding an explicit save.
            if ((ex.Message ?? "").IndexOf("Without Save", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                log("warning", $"SynchronizeWithCentral first attempt failed ({ex.Message}); trying save+retry.");
                TrySaveBeforeSwc(doc, swc, log, phase: "retry_after_without_save");
                try
                {
                    var twcRetry = new TransactWithCentralOptions();
                    var optsRetry = new SynchronizeWithCentralOptions
                    {
                        Comment = "Design Automation — run_revit_automation (retry)",
                        SaveLocalBefore = true,
                        SaveLocalAfter = true,
                    };
                    doc.SynchronizeWithCentral(twcRetry, optsRetry);
                    swc["status"] = "ok_after_retry";
                    swc["retry_reason"] = "STC Without Save";
                    log("info", "SynchronizeWithCentral completed on retry after explicit save.");
                    return true;
                }
                catch (Exception retryEx)
                {
                    swc["status"] = "failed";
                    swc["error"] = retryEx.Message;
                    swc["retry_reason"] = "STC Without Save";
                    log("warning", $"SynchronizeWithCentral retry failed: {retryEx.Message}");
                    return true;
                }
            }

            swc["status"] = "failed";
            swc["error"] = ex.Message;
            log("warning", $"SynchronizeWithCentral failed: {ex.Message}");
            return true;
        }
    }

    private static void TrySaveBeforeSwc(Document doc, JObject swc, Action<string, string> log, string phase)
    {
        try
        {
            doc.Save();
            swc[$"save_{phase}"] = "ok";
            log("info", $"Document save succeeded ({phase}).");
        }
        catch (Exception saveEx)
        {
            swc[$"save_{phase}"] = "failed";
            swc[$"save_{phase}_error"] = saveEx.Message;
            log("warning", $"Document save failed ({phase}): {saveEx.Message}");
        }
    }

    private static Parameter ResolveParameter(Element target, string parameterName, JObject sharedGuidMap)
    {
        var matches = FindParametersWithDefinitionName(target, parameterName);
        if (matches.Count == 1)
            return matches[0];
        if (matches.Count > 1)
            return PickParameterWhenDuplicateName(matches, sharedGuidMap, parameterName);
        var direct = target.LookupParameter(parameterName);
        if (direct != null)
            return direct;

        // Fallback: many Revit schemas store editable shared params on the element TYPE.
        var typeId = target.GetTypeId();
        if (typeId == ElementId.InvalidElementId)
            return null;
        var typeEl = target.Document.GetElement(typeId);
        if (typeEl == null)
            return null;

        var typeMatches = FindParametersWithDefinitionName(typeEl, parameterName);
        if (typeMatches.Count == 1)
            return typeMatches[0];
        if (typeMatches.Count > 1)
            return PickParameterWhenDuplicateName(typeMatches, sharedGuidMap, parameterName);
        return typeEl.LookupParameter(parameterName);
    }

    private static List<Parameter> FindParametersWithDefinitionName(Element target, string parameterName)
    {
        var list = new List<Parameter>();
        if (string.IsNullOrWhiteSpace(parameterName)) return list;
        foreach (Parameter p in target.Parameters)
        {
            if (p?.Definition?.Name == null) continue;
            if (!string.Equals(p.Definition.Name, parameterName, StringComparison.Ordinal))
                continue;
            list.Add(p);
        }

        return list;
    }

    private static Parameter PickParameterWhenDuplicateName(
        List<Parameter> matches,
        JObject sharedGuidMap,
        string parameterName)
    {
        if (sharedGuidMap != null)
        {
            var gtok = sharedGuidMap[parameterName];
            var gs = gtok?.ToString();
            if (!string.IsNullOrWhiteSpace(gs) && Guid.TryParse(gs, out var sg))
            {
                foreach (var p in matches)
                {
                    if (p.Definition is ExternalDefinition ed && ed.GUID == sg)
                        return p;
                }
            }
        }

        foreach (var p in matches)
        {
            if (!p.IsReadOnly)
                return p;
        }

        return matches.Count > 0 ? matches[0] : null;
    }

    private static Element FindByExternalId(Document doc, string externalId)
    {
        if (doc == null || string.IsNullOrWhiteSpace(externalId))
            return null;

        // Primary path: payload externalIds are usually Revit UniqueId values.
        var directByUniqueId = doc.GetElement(externalId);
        if (directByUniqueId != null)
            return directByUniqueId;

        // Secondary path: many UniqueIds end with an 8-hex ElementId segment.
        var dash = externalId.LastIndexOf('-');
        if (dash >= 0 && dash < externalId.Length - 1)
        {
            var tail = externalId.Substring(dash + 1);
            if (int.TryParse(tail, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var idInt))
            {
                var byElementId = doc.GetElement(new ElementId((long)idInt));
                if (byElementId != null)
                    return byElementId;
            }
        }

        // Legacy fallback for models that mirror these values into a custom parameter.
        foreach (var e in new FilteredElementCollector(doc).WhereElementIsNotElementType())
        {
            var p = e.LookupParameter("External ID");
            if (p != null && string.Equals(p.AsString(), externalId, StringComparison.OrdinalIgnoreCase))
                return e;
        }

        return null;
    }
}
