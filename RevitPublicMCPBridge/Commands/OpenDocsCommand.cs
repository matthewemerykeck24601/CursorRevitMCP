using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

namespace RevitPublicMCPBridge.Commands;

[Transaction(TransactionMode.Manual)]
public sealed class OpenDocsCommand : BaseBridgeCommand
{
    protected override Result ExecuteSafe(
        ExternalCommandData commandData,
        ref string message)
    {
        var url = Services.BridgeContext.Instance.SettingsStore.Current.HelpUrl;
        if (string.IsNullOrWhiteSpace(url))
        {
            url = "https://help.autodesk.com/view/RVT/2027/ENU/";
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            TaskDialog.Show(
                "Public MCP Server - Help",
                $"Could not open browser.\n\n{ex.Message}\n\nURL: {url}");
            return Result.Failed;
        }
    }
}
