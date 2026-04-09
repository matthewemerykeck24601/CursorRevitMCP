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
/// Local test entry (IExternalCommand). For Design Automation, switch to IExternalDBApplication
/// or the DA entry class pattern required by your Activity.
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

        // TODO: In DA, read from DesignAutomationData.GetArguments() or workitem input zip path.
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

        using var tx = new Transaction(doc, "Apply cached analysis + parameter patches");
        tx.Start();
        try
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
                    {
                        p.Set(mark);
                    }
                }
            }

            ApplyParameterPatches(doc, root, sharedGuidMap);

            // TODO: SynchronizeWithCentral — only when doc.IsWorkshared; ACC cloud workflow in DA bundle.
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

    /// <summary>
    /// Generic instance-parameter writes: JSON drives names/values; same code for every parameter.
    /// Root <c>parameterPatches</c> or <c>additional_updates.parameterPatches</c>:
    /// [{ "externalIds": ["..."], "set": { "CONTROL_MARK": "", "COMMENTS": "run-42" } }, ...]
    /// </summary>
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

    /// <summary>
    /// Resolve by parameter definition name first (standard Metromont names). If more than one parameter
    /// on the element shares that definition name, require <c>sharedParameterGuidMap[parameterName]</c>
    /// (Revit shared GUID string) to pick the correct <see cref="ExternalDefinition"/>.
    /// </summary>
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

        switch (p.StorageType)
        {
            case StorageType.String:
                var s = valueToken.Type == JTokenType.Null ? "" : valueToken.ToString();
                p.Set(s ?? "");
                break;
            case StorageType.Double:
                if (valueToken.Type == JTokenType.Float || valueToken.Type == JTokenType.Integer)
                    p.Set(valueToken.Value<double>());
                else if (double.TryParse(valueToken.ToString(), System.Globalization.NumberStyles.Any,
                             System.Globalization.CultureInfo.InvariantCulture, out var d))
                    p.Set(d);
                break;
            case StorageType.Integer:
                if (valueToken.Type == JTokenType.Integer || valueToken.Type == JTokenType.Float)
                    p.Set(valueToken.Value<int>());
                else if (int.TryParse(valueToken.ToString(), out var i))
                    p.Set(i);
                break;
            case StorageType.ElementId:
                if (valueToken.Type == JTokenType.Integer)
                    p.Set(new ElementId(valueToken.Value<long>()));
                break;
            default:
                break;
        }
    }

    private static Element FindByExternalId(Document doc, string externalId)
    {
        foreach (var e in new FilteredElementCollector(doc).WhereElementIsNotElementType())
        {
            var p = e.LookupParameter("External ID");
            if (p != null && string.Equals(p.AsString(), externalId, StringComparison.Ordinal))
            {
                return e;
            }
        }

        return null;
    }
}
