#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json.Linq;

namespace Metromont.RevitMarkWorkitem;

/// <summary>
/// Design Automation / local command: dispatches on <c>operation</c> and <c>skip_analysis</c>.
/// - <c>modify_parameters</c>: <c>parameter_updates</c> + <c>parameterPatches</c> only (no mark grouping).
/// - <c>apply_marks</c>: legacy <c>marks[]</c> loop for CONTROL_MARK.
/// - <c>apply_marks_and_modify</c>: both.
/// When <c>skip_analysis</c> is true, mark application is always skipped.
/// </summary>
[Transaction(TransactionMode.Manual)]
public class MarkWorkitemApp : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var uiApp = commandData.Application;
        var doc = uiApp.ActiveUIDocument?.Document;
        if (doc == null)
        {
            message = "No active document.";
            return Result.Failed;
        }

        var payloadPath = Environment.GetEnvironmentVariable("MARK_PAYLOAD_JSON")
                          ?? Path.Combine(Path.GetTempPath(), "mark-payload.json");
        if (!File.Exists(payloadPath))
        {
            message = $"Payload not found: {payloadPath}";
            return Result.Failed;
        }

        var json = File.ReadAllText(payloadPath);
        var root = JObject.Parse(json);
        var marks = root["marks"] as JArray ?? new JArray();
        var sharedGuidMap = root["sharedParameterGuidMap"] as JObject;
        var skipAnalysis = root["skip_analysis"]?.Value<bool>() == true;
        var op = root["operation"]?.ToString()?.Trim() ?? "";

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

        var runMarks = !skipAnalysis && (string.Equals(op, "apply_marks", StringComparison.OrdinalIgnoreCase)
                                         || string.Equals(op, "apply_marks_and_modify", StringComparison.OrdinalIgnoreCase));
        var runModify = string.Equals(op, "modify_parameters", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(op, "apply_marks_and_modify", StringComparison.OrdinalIgnoreCase);

        var txLabel = runMarks && runModify
            ? "DA: marks + parameters"
            : runMarks
                ? "DA: apply marks"
                : "DA: modify parameters";
        using var tx = new Transaction(doc, txLabel);
        tx.Start();
        try
        {
            if (runMarks)
            {
                foreach (var m in marks)
                {
                    var mark = m?["control_mark"]?.ToString();
                    var extIds = m?["externalIds"]?.ToObject<List<string>>() ?? new List<string>();
                    if (string.IsNullOrEmpty(mark)) continue;

                    foreach (var ext in extIds)
                    {
                        if (string.IsNullOrWhiteSpace(ext)) continue;
                        var target = FindByExternalId(doc, ext);
                        if (target == null) continue;
                        var p = ResolveParameter(target, "CONTROL_MARK", sharedGuidMap);
                        if (p is { IsReadOnly: false })
                            p.Set(mark);
                    }
                }
            }

            if (runModify)
            {
                ApplyParameterUpdates(doc, root, sharedGuidMap);
                ApplyParameterPatches(doc, root, sharedGuidMap);
            }

            tx.Commit();
        }
        catch (Exception ex)
        {
            tx.RollBack();
            message = ex.Message;
            return Result.Failed;
        }

        return Result.Succeeded;
    }

    private static void ApplyParameterUpdates(Document doc, JObject root, JObject sharedGuidMap)
    {
        if (root["parameter_updates"] is not JArray arr) return;
        foreach (var item in arr)
        {
            if (item is not JObject row) continue;
            var extIds = row["externalIds"]?.ToObject<List<string>>() ?? new List<string>();
            var paramName = row["paramName"]?.ToString()?.Trim();
            var action = row["action"]?.ToString()?.Trim()?.ToLowerInvariant();
            if (string.IsNullOrEmpty(paramName) || string.IsNullOrEmpty(action)) continue;

            foreach (var ext in extIds)
            {
                if (string.IsNullOrWhiteSpace(ext)) continue;
                var target = FindByExternalId(doc, ext);
                if (target == null) continue;
                var p = ResolveParameter(target, paramName, sharedGuidMap);
                if (p == null || p.IsReadOnly) continue;

                if (action == "clear")
                    ClearParameterValue(p);
                else if (action == "set")
                    SetParameterFromToken(p, row["value"]);
                else if (action == "toggle")
                    ToggleParameterValue(p);
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

    private static void ApplyParameterPatches(Document doc, JObject root, JObject sharedGuidMap)
    {
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
            if (item is not JObject row) continue;
            var extIds = row["externalIds"]?.ToObject<List<string>>() ?? new List<string>();
            var setObj = row["set"] as JObject;
            if (setObj == null || setObj.Count == 0) continue;

            foreach (var ext in extIds)
            {
                if (string.IsNullOrWhiteSpace(ext)) continue;
                var target = FindByExternalId(doc, ext);
                if (target == null) continue;
                foreach (var prop in setObj.Properties())
                    ApplyOneParameter(target, prop.Name, prop.Value, sharedGuidMap);
            }
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

    private static void ApplyOneParameter(Element target, string parameterName, JToken valueToken, JObject sharedGuidMap)
    {
        if (string.IsNullOrWhiteSpace(parameterName)) return;
        var p = ResolveParameter(target, parameterName, sharedGuidMap);
        if (p == null || p.IsReadOnly) return;
        SetParameterFromToken(p, valueToken);
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
