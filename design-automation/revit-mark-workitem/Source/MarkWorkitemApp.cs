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

        using var tx = new Transaction(doc, "Apply CONTROL_MARK from cached analysis");
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
                    // TODO: Replace with robust lookup (Shared parameters, IFC GUID, etc.)
                    var target = FindByExternalId(doc, ext);
                    if (target == null) continue;
                    var p = target.LookupParameter("CONTROL_MARK");
                    if (p is { IsReadOnly: false })
                    {
                        p.Set(mark);
                    }
                }
            }

            // TODO: SynchronizeWithCentral — only when doc.IsWorkshared; handle conflicts / relinq.
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
