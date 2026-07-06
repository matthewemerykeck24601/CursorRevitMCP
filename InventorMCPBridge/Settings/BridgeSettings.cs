namespace InventorMCPBridge.Settings;

public sealed class BridgeSettings
{
    public bool EnableLocalApiGateway { get; set; } = true;

    public int LocalApiGatewayPort { get; set; } = 8776;

    public bool AutoStartOnLaunch { get; set; } = true;

    public bool LogVerboseGatewayPayloads { get; set; } = false;

    public string HelpUrl { get; set; } =
        "https://www.autodesk.com/products/informed-design/overview";
}
