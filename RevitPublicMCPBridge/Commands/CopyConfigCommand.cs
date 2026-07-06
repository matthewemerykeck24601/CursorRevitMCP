using System.Text;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

namespace RevitPublicMCPBridge.Commands;

[Transaction(TransactionMode.Manual)]
public sealed class CopyConfigCommand : BaseBridgeCommand
{
    protected override Result ExecuteSafe(
        ExternalCommandData commandData,
        ref string message)
    {
        var status = Services.BridgeContext.Instance.Controller.GetStatus();
        var endpoint = status.Endpoint;
        var gateway = Services.BridgeContext.Instance.GatewayService;

        var sb = new StringBuilder();
        sb.AppendLine("Revit Public MCP Server connection details");
        sb.AppendLine("------------------------------------------");
        sb.AppendLine($"Running: {(status.IsRunning ? "Yes" : "No")}");
        sb.AppendLine($"Endpoint: {endpoint}");
        sb.AppendLine($"Gateway: {gateway.BaseUrl}");
        sb.AppendLine();
        sb.AppendLine("Cursor / generic MCP hint:");
        sb.AppendLine($"- Use the Revit Public MCP endpoint: {endpoint}");
        sb.AppendLine($"- Use 2027 community write bridge base URL: {gateway.BaseUrl}");
        sb.AppendLine("- Ensure Revit is open and server is running.");

        try
        {
            Clipboard.SetText(sb.ToString());
            TaskDialog.Show(
                "Public MCP Server - Copy Config",
                "Connection details copied to clipboard.");
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            TaskDialog.Show(
                "Public MCP Server - Copy Config",
                $"Could not access clipboard.\n\n{ex.Message}\n\nDetails:\n{sb}");
            return Result.Failed;
        }
    }
}
