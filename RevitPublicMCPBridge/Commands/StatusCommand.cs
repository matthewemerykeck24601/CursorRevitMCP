using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

namespace RevitPublicMCPBridge.Commands;

[Transaction(TransactionMode.Manual)]
public sealed class StatusCommand : BaseBridgeCommand
{
    protected override Result ExecuteSafe(
        ExternalCommandData commandData,
        ref string message)
    {
        var status = Services.BridgeContext.Instance.Controller.GetStatus();
        var gateway = Services.BridgeContext.Instance.GatewayService;

        var content =
            $"Running: {(status.IsRunning ? "Yes" : "No")}\n" +
            $"Mode: {status.ControlMode}\n" +
            $"Port: {status.Port}\n" +
            $"Endpoint: {status.Endpoint}\n" +
            $"Gateway: {(gateway.IsRunning ? "Running" : "Stopped")} ({gateway.BaseUrl})\n" +
            $"Executable: {status.ExecutablePath}\n\n" +
            $"Details:\n{status.Details}";

        TaskDialog.Show("Public MCP Server - Status", content);
        return Result.Succeeded;
    }
}
