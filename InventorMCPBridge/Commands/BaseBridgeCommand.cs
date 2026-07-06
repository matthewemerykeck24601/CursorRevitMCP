using System.Windows;
using InventorMCPBridge.Services;

namespace InventorMCPBridge.Commands;

public abstract class BaseBridgeCommand
{
    public void Execute()
    {
        try
        {
            ExecuteSafe();
        }
        catch (Exception ex)
        {
            BridgeContext.Logger.Error($"{GetType().Name} failed.", ex);
            MessageBox.Show(
                $"Command failed:{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                "Inventor MCP Bridge",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    protected abstract void ExecuteSafe();
}
