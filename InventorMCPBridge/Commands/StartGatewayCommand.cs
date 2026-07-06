using System.Windows;
using InventorMCPBridge.Services;

namespace InventorMCPBridge.Commands;

public sealed class StartGatewayCommand : BaseBridgeCommand
{
    protected override void ExecuteSafe()
    {
        BridgeContext.Instance.StartGateway();
        MessageBox.Show(
            $"Inventor gateway listening at {BridgeContext.Instance.Gateway.BaseUrl}",
            "Inventor MCP Bridge",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
