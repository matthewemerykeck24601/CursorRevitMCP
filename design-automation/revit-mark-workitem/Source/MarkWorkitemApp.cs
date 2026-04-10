#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Metromont.RevitMarkWorkitem;

/// <summary>
/// Design Automation / local command: dispatches on <c>operation</c> and <c>skip_analysis</c>.
/// <list type="bullet">
/// <item><c>modify_parameters</c> — <c>parameter_updates</c>, <c>parameterPatches</c>, and/or <c>cached_selection</c> + <c>updates</c>.</item>
/// <item><c>run_mark_analysis</c> / <c>apply_marks</c> — <c>marks[]</c> → CONTROL_MARK (and related).</item>
/// <item><c>apply_marks_and_modify</c> — both paths.</item>
/// <item><c>clear_cache</c> — no Revit edits; audit-only stub for future hooks.</item>
/// </list>
/// When <c>skip_analysis</c> is true, mark application is never run (execution-only edits).
/// </summary>
[Transaction(TransactionMode.Manual)]
public class MarkWorkitemApp : IExternalCommand
{
    private const string EnvPayload = "MARK_PAYLOAD_JSON";
    private const string EnvAudit = "MARK_AUDIT_JSON";
    private const string EnvSwc = "MARK_SWC";

    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var uiApp = commandData.Application;
        var doc = uiApp.ActiveUIDocument?.Document;
        if (doc == null)
        {
            message = "No active document.";
            return Result.Failed;
        }

        var payloadPath = Environment.GetEnvironmentVariable(EnvPayload)
                          ?? Path.Combine(Path.GetTempPath(), "mark-payload.json");
        if (!File.Exists(payloadPath))
        {
            message = $"Payload not found: {payloadPath}";
            return Result.Failed;
        }

        var json = File.ReadAllText(payloadPath);
        var root = JObject.Parse(json);
        ExpandCachedSelectionUpdates(root);

        var auditPath = Environment.GetEnvironmentVariable(EnvAudit)
                        ?? Path.Combine(Path.GetTempPath(), "mark-audit.json");
        var audit = NewAuditRoot(root);

        var marks = root["marks"] as JArray ?? new JArray();
        var sharedGuidMap = root["sharedParameterGuidMap"] as JObject;
        var skipAnalysis = root["skip_analysis"]?.Value<bool>() == true;
        var op = root["operation"]?.ToString()?.Trim() ?? "";

        if (string.Equals(op, "clear_cache", StringComparison.OrdinalIgnoreCase))
        {
            audit["result"] = "clear_cache_no_op";
            audit["note"] = "No model changes; reserved for future cache invalidation hooks.";
            WriteAudit(auditPath, audit);
            return Result.Succeeded;
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

        if (!runMarks && !runModify)
        {
            audit["result"] = "no_op";
            audit["warning"] = $"Unknown or empty operation after resolution: {op}";
            WriteAudit(auditPath, audit);
            message = audit["warning"].ToString();
            return Result.Failed;
        }

        var txLabel = runMarks && runModify
            ? "DA: marks + parameters"
            : runMarks
                ? "DA: run mark analysis / apply marks"
                : "DA: modify parameters";
        using var tx = new Transaction(doc, txLabel);
        tx.Start();
        try
        {
            if (runMarks)
                RunMarkPass(doc, marks, sharedGuidMap, audit);

            if (runModify)
            {
                ApplyParameterUpdates(doc, root, sharedGuidMap, audit);
                ApplyParameterPatches(doc, root, sharedGuidMap, audit);
            }

            tx.Commit();
        }
        catch (Exception ex)
        {
            tx.RollBack();
            audit["transaction"] = "rolled_back";
            audit["error"] = ex.Message;
            WriteAudit(auditPath, audit);
            message = ex.Message;
            return Result.Failed;
        }

        audit["transaction"] = "committed";
        TrySynchronizeWithCentral(doc, audit);
        WriteAudit(auditPath, audit);
        return Result.Succeeded;
    }

    private static JObject NewAuditRoot(JObject root)
    {
        return new JObject
        {
            ["timestamp_utc"] = DateTime.UtcNow.ToString("o"),
            ["payload_operation"] = root["operation"]?.ToString(),
            ["skip_analysis"] = root["skip_analysis"]?.Value<bool>() ?? false,
            ["cache_id"] = root["cache_id"]?.ToString()
                           ?? root["cached_selection"]?["cache_id"]?.ToString(),
            ["marks"] = new JObject
            {
                ["groups_processed"] = 0,
                ["external_id_attempts"] = 0,
                ["elements_matched"] = 0,
                ["elements_missed"] = 0,
                ["parameters_set"] = 0,
                ["parameters_skipped_readonly"] = 0,
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
        };
    }

    private static void WriteAudit(string path, JObject audit)
    {
        try
        {
            File.WriteAllText(path, audit.ToString(Formatting.Indented));
        }
        catch
        {
            /* best-effort for DA sandbox */
        }
    }

    /// <summary>
    /// Expands <c>cached_selection.externalIds</c> + root <c>updates[]</c> into canonical <c>parameter_updates</c> rows.
    /// </summary>
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
            var paramName = row["paramName"]?.ToString()?.Trim();
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

    private static bool IsMarkOperation(string op) =>
        string.Equals(op, "apply_marks", StringComparison.OrdinalIgnoreCase)
        || string.Equals(op, "apply_marks_and_modify", StringComparison.OrdinalIgnoreCase)
        || string.Equals(op, "run_mark_analysis", StringComparison.OrdinalIgnoreCase);

    private static bool IsModifyOperation(string op) =>
        string.Equals(op, "modify_parameters", StringComparison.OrdinalIgnoreCase)
        || string.Equals(op, "apply_marks_and_modify", StringComparison.OrdinalIgnoreCase);

    private static void RunMarkPass(Document doc, JArray marks, JObject sharedGuidMap, JObject audit)
    {
        var sec = (JObject)audit["marks"];
        foreach (var m in marks)
        {
            var mark = m?["control_mark"]?.ToString();
            var extIds = m?["externalIds"]?.ToObject<List<string>>() ?? new List<string>();
            if (string.IsNullOrEmpty(mark)) continue;
            sec["groups_processed"] = sec["groups_processed"].Value<int>() + 1;

            foreach (var ext in extIds)
            {
                if (string.IsNullOrWhiteSpace(ext)) continue;
                sec["external_id_attempts"] = sec["external_id_attempts"].Value<int>() + 1;
                var target = FindByExternalId(doc, ext);
                if (target == null)
                {
                    sec["elements_missed"] = sec["elements_missed"].Value<int>() + 1;
                    AppendMiss(sec, ext, "CONTROL_MARK", "element_not_found");
                    continue;
                }

                sec["elements_matched"] = sec["elements_matched"].Value<int>() + 1;
                var p = ResolveParameter(target, "CONTROL_MARK", sharedGuidMap);
                if (p == null)
                {
                    AppendMiss(sec, ext, "CONTROL_MARK", "parameter_not_found");
                    continue;
                }

                if (p.IsReadOnly)
                {
                    sec["parameters_skipped_readonly"] = sec["parameters_skipped_readonly"].Value<int>() + 1;
                    AppendMiss(sec, ext, "CONTROL_MARK", "read_only");
                    continue;
                }

                p.Set(mark);
                sec["parameters_set"] = sec["parameters_set"].Value<int>() + 1;
            }
        }
    }

    private static void AppendMiss(JObject section, string externalId, string paramName, string reason)
    {
        var arr = section["misses"] as JArray;
        if (arr == null)
        {
            arr = new JArray();
            section["misses"] = arr;
        }

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

    private static void ApplyParameterUpdates(Document doc, JObject root, JObject sharedGuidMap, JObject audit)
    {
        var sec = (JObject)audit["modify"];
        if (root["parameter_updates"] is not JArray arr) return;

        foreach (var item in arr)
        {
            sec["parameter_update_rows"] = sec["parameter_update_rows"].Value<int>() + 1;
            if (item is not JObject row) continue;
            var extIds = row["externalIds"]?.ToObject<List<string>>() ?? new List<string>();
            var paramName = row["paramName"]?.ToString()?.Trim();
            var action = row["action"]?.ToString()?.Trim()?.ToLowerInvariant();
            if (string.IsNullOrEmpty(paramName) || string.IsNullOrEmpty(action)) continue;

            foreach (var ext in extIds)
            {
                if (string.IsNullOrWhiteSpace(ext)) continue;
                sec["external_id_attempts"] = sec["external_id_attempts"].Value<int>() + 1;
                var target = FindByExternalId(doc, ext);
                if (target == null)
                {
                    sec["elements_missed"] = sec["elements_missed"].Value<int>() + 1;
                    AppendMiss(sec, ext, paramName, "element_not_found");
                    continue;
                }

                sec["elements_matched"] = sec["elements_matched"].Value<int>() + 1;
                var p = ResolveParameter(target, paramName, sharedGuidMap);
                if (p == null)
                {
                    sec["parameter_actions_failed"] = sec["parameter_actions_failed"].Value<int>() + 1;
                    AppendFailure(sec, ext, paramName, action, "parameter_not_found");
                    continue;
                }

                if (p.IsReadOnly)
                {
                    sec["parameter_actions_failed"] = sec["parameter_actions_failed"].Value<int>() + 1;
                    AppendFailure(sec, ext, paramName, action, "read_only");
                    continue;
                }

                try
                {
                    if (action == "clear")
                        ClearParameterValue(p);
                    else if (action == "set")
                        SetParameterFromToken(p, row["value"]);
                    else if (action == "toggle")
                        ToggleParameterValue(p);
                    else
                    {
                        sec["parameter_actions_failed"] = sec["parameter_actions_failed"].Value<int>() + 1;
                        AppendFailure(sec, ext, paramName, action, "unknown_action");
                        continue;
                    }

                    sec["parameter_actions_ok"] = sec["parameter_actions_ok"].Value<int>() + 1;
                }
                catch (Exception ex)
                {
                    sec["parameter_actions_failed"] = sec["parameter_actions_failed"].Value<int>() + 1;
                    AppendFailure(sec, ext, paramName, action, "exception:" + ex.Message);
                }
            }
        }
    }

    private static void ClearParameterValue(Parameter p)
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

    private static void ToggleParameterValue(Parameter p)
    {
        if (p.StorageType == StorageType.Integer)
        {
            var v = p.AsInteger();
            p.Set(v == 0 ? 1 : 0);
        }
        else if (p.StorageType == StorageType.String)
        {
            var s = p.AsString() ?? "";
            if (string.Equals(s, "Yes", StringComparison.OrdinalIgnoreCase))
                p.Set("No");
            else if (string.Equals(s, "No", StringComparison.OrdinalIgnoreCase))
                p.Set("Yes");
        }
    }

    private static void SetParameterFromToken(Parameter p, JToken valueToken)
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
                break;
            case StorageType.Integer:
                if (valueToken != null && (valueToken.Type == JTokenType.Integer || valueToken.Type == JTokenType.Float))
                    p.Set(valueToken.Value<int>());
                else if (valueToken != null && int.TryParse(valueToken.ToString(), out var i))
                    p.Set(i);
                break;
            case StorageType.ElementId:
                if (valueToken != null && valueToken.Type == JTokenType.Integer)
                    p.Set(new ElementId(valueToken.Value<long>()));
                break;
            default:
                break;
        }
    }

    private static void ApplyParameterPatches(Document doc, JObject root, JObject sharedGuidMap, JObject audit)
    {
        var sec = (JObject)audit["patches"];
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
            sec["rows"] = sec["rows"].Value<int>() + 1;
            if (item is not JObject row) continue;
            var extIds = row["externalIds"]?.ToObject<List<string>>() ?? new List<string>();
            var setObj = row["set"] as JObject;
            if (setObj == null || setObj.Count == 0) continue;

            foreach (var ext in extIds)
            {
                if (string.IsNullOrWhiteSpace(ext)) continue;
                sec["external_id_attempts"] = sec["external_id_attempts"].Value<int>() + 1;
                var target = FindByExternalId(doc, ext);
                if (target == null)
                {
                    sec["elements_missed"] = sec["elements_missed"].Value<int>() + 1;
                    continue;
                }

                sec["elements_matched"] = sec["elements_matched"].Value<int>() + 1;
                foreach (var prop in setObj.Properties())
                {
                    var p = ResolveParameter(target, prop.Name, sharedGuidMap);
                    if (p == null)
                    {
                        sec["parameters_skipped"] = sec["parameters_skipped"].Value<int>() + 1;
                        continue;
                    }

                    if (p.IsReadOnly)
                    {
                        sec["parameters_skipped"] = sec["parameters_skipped"].Value<int>() + 1;
                        continue;
                    }

                    try
                    {
                        SetParameterFromToken(p, prop.Value);
                        sec["parameters_set"] = sec["parameters_set"].Value<int>() + 1;
                    }
                    catch
                    {
                        sec["parameters_skipped"] = sec["parameters_skipped"].Value<int>() + 1;
                    }
                }
            }
        }
    }

    private static void TrySynchronizeWithCentral(Document doc, JObject audit)
    {
        var swc = (JObject)audit["swc"];
        var flag = Environment.GetEnvironmentVariable(EnvSwc);
        if (!string.Equals(flag, "true", StringComparison.OrdinalIgnoreCase))
        {
            swc["status"] = "skipped_not_enabled";
            swc["hint"] = $"Set {EnvSwc}=true to attempt SynchronizeWithCentral after commit.";
            return;
        }

        if (!doc.IsWorkshared)
        {
            swc["attempted"] = true;
            swc["status"] = "skipped_not_workshared";
            return;
        }

        try
        {
            swc["attempted"] = true;
            var twc = new TransactWithCentralOptions();
            var opts = new SynchronizeWithCentralOptions
            {
                Comment = "Design Automation MCP run",
                SaveLocalBefore = true,
                SaveLocalAfter = true,
            };
            doc.SynchronizeWithCentral(twc, opts);
            swc["status"] = "ok";
        }
        catch (Exception ex)
        {
            swc["status"] = "failed";
            swc["error"] = ex.Message;
        }
    }

    private static Parameter ResolveParameter(Element target, string parameterName, JObject sharedGuidMap)
    {
        var matches = FindParametersWithDefinitionName(target, parameterName);
        if (matches.Count == 1)
            return matches[0];
        if (matches.Count > 1)
            return PickParameterWhenDuplicateName(matches, sharedGuidMap, parameterName);
        return target.LookupParameter(parameterName);
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
        if (sharedGuidMap == null) return null;
        var gtok = sharedGuidMap[parameterName];
        var gs = gtok?.ToString();
        if (string.IsNullOrWhiteSpace(gs) || !Guid.TryParse(gs, out var sg)) return null;
        foreach (var p in matches)
        {
            if (p.Definition is ExternalDefinition ed && ed.GUID == sg)
                return p;
        }

        return null;
    }

    private static Element FindByExternalId(Document doc, string externalId)
    {
        foreach (var e in new FilteredElementCollector(doc).WhereElementIsNotElementType())
        {
            var p = e.LookupParameter("External ID");
            if (p != null && string.Equals(p.AsString(), externalId, StringComparison.Ordinal))
                return e;
        }

        return null;
    }
}
