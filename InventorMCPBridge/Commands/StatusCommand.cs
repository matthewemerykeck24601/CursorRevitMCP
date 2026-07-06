using System.Windows;
using InventorMCPBridge.Services;

namespace InventorMCPBridge.Commands;

public sealed class StatusCommand : BaseBridgeCommand
{
    protected override void ExecuteSafe()
    {
        var gateway = BridgeContext.Instance.Gateway;
        var status = gateway.IsRunning ? "Running" : "Stopped";
        var message = string.Join(
            Environment.NewLine,
            $"Gateway: {status}",
            $"Base URL: {gateway.BaseUrl}",
            $"Port: {gateway.Port}");

        MessageBox.Show(
            message,
            "Inventor MCP Bridge Status",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
