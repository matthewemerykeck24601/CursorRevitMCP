using System.Windows;
using InventorMCPBridge.Services;

namespace InventorMCPBridge.Commands;

public sealed class BridgeLinkCommand : BaseBridgeCommand
{
    protected override void ExecuteSafe()
    {
        var gateway = BridgeContext.Instance.Gateway;
        if (!gateway.IsRunning)
        {
            BridgeContext.Instance.StartGateway();
        }

        var status = gateway.IsRunning ? "Connected" : "Not connected";
        MessageBox.Show(
            $"Bridge status: {status}{Environment.NewLine}Gateway: {gateway.BaseUrl}",
            "Inventor MCP Bridge",
            MessageBoxButton.OK,
            gateway.IsRunning ? MessageBoxImage.Information : MessageBoxImage.Warning);
    }
}
