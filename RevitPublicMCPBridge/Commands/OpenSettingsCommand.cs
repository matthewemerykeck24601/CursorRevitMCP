using System.Windows.Interop;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using RevitPublicMCPBridge.UI;

namespace RevitPublicMCPBridge.Commands;

[Transaction(TransactionMode.Manual)]
public sealed class OpenSettingsCommand : BaseBridgeCommand
{
    protected override Result ExecuteSafe(
        ExternalCommandData commandData,
        ref string message)
    {
        var window = new SettingsWindow(Services.BridgeContext.Instance.SettingsStore);
        var helper = new WindowInteropHelper(window)
        {
            Owner = commandData.Application.MainWindowHandle,
        };

        window.ShowDialog();
        return Result.Succeeded;
    }
}
