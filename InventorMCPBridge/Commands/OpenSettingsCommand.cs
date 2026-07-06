using InventorMCPBridge.Services;
using InventorMCPBridge.UI;

namespace InventorMCPBridge.Commands;

public sealed class OpenSettingsCommand : BaseBridgeCommand
{
    protected override void ExecuteSafe()
    {
        var window = new SettingsWindow(BridgeContext.Instance.SettingsStore);
        var saved = window.ShowDialog();
        if (saved == true)
        {
            BridgeContext.Instance.StopGateway();
            BridgeContext.Instance.StartGateway();
        }
    }
}
