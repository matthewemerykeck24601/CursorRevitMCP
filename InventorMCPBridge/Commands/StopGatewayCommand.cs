using System.Windows;
using InventorMCPBridge.Services;

namespace InventorMCPBridge.Commands;

public sealed class StopGatewayCommand : BaseBridgeCommand
{
    protected override void ExecuteSafe()
    {
        BridgeContext.Instance.StopGateway();
        MessageBox.Show(
            "Inventor gateway stopped.",
            "Inventor MCP Bridge",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
