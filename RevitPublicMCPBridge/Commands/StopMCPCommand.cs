using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

namespace RevitPublicMCPBridge.Commands;

[Transaction(TransactionMode.Manual)]
public sealed class StopMCPCommand : BaseBridgeCommand
{
    protected override Result ExecuteSafe(
        ExternalCommandData commandData,
        ref string message)
    {
        var result = Services.BridgeContext.Instance.Controller.StopServer();
        var title = "Public MCP Server - Stop";
        if (result.Success)
        {
            TaskDialog.Show(title, result.Message);
            return Result.Succeeded;
        }

        TaskDialog.Show(title, $"Stop failed:\n\n{result.Message}");
        message = result.Message;
        return Result.Failed;
    }
}
