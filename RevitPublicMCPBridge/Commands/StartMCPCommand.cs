using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

namespace RevitPublicMCPBridge.Commands;

[Transaction(TransactionMode.Manual)]
public sealed class StartMCPCommand : BaseBridgeCommand
{
    protected override Result ExecuteSafe(
        ExternalCommandData commandData,
        ref string message)
    {
        var result = Services.BridgeContext.Instance.Controller.StartServer();
        var title = "Public MCP Server - Start";
        if (result.Success)
        {
            TaskDialog.Show(title, result.Message);
            return Result.Succeeded;
        }

        TaskDialog.Show(title, $"Start failed:\n\n{result.Message}");
        message = result.Message;
        return Result.Failed;
    }
}
