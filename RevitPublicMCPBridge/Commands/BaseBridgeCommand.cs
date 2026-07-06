using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitPublicMCPBridge.Commands;

[Transaction(TransactionMode.Manual)]
public abstract class BaseBridgeCommand : IExternalCommand
{
    public Result Execute(
        ExternalCommandData commandData,
        ref string message,
        ElementSet elements)
    {
        try
        {
            return ExecuteSafe(commandData, ref message);
        }
        catch (Exception ex)
        {
            Services.BridgeContext.Logger.Error($"{GetType().Name} failed.", ex);
            message = ex.Message;
            TaskDialog.Show(
                "Revit Public MCP Bridge",
                $"Command failed:\n\n{ex.Message}");
            return Result.Failed;
        }
    }

    protected abstract Result ExecuteSafe(
        ExternalCommandData commandData,
        ref string message);
}
