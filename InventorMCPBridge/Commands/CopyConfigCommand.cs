using System.Windows;
using InventorMCPBridge.Services;

namespace InventorMCPBridge.Commands;

public sealed class CopyConfigCommand : BaseBridgeCommand
{
    protected override void ExecuteSafe()
    {
        var gateway = BridgeContext.Instance.Gateway;
        var config = $$"""
        "inventor-mcp": {
          "command": "cmd",
          "args": ["/c", "npx", "-y", "tsx", "D:\\CursorRevitMCP\\inventor-mcp\\src\\index.ts"],
          "env": {
            "INVENTOR_BRIDGE_URL": "{{gateway.BaseUrl}}",
            "INVENTOR_PARAM_SOURCE": "excel",
            "INVENTOR_EXCEL_PATH": "D:\\CursorRevitMCP\\data\\inventor-parameters.xlsx"
          }
        }
        """;

        Clipboard.SetText(config);
        MessageBox.Show(
            "Inventor MCP config copied to clipboard.",
            "Inventor MCP Bridge",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
