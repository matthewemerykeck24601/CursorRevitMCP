namespace RevitPublicMCPBridge.Settings;

public sealed class BridgeSettings
{
    public bool EnableLocalApiGateway { get; set; } = true;

    public int LocalApiGatewayPort { get; set; } = 8766;

    public bool EnableProcessControl { get; set; } = false;

    public bool AutoStartOnLaunch { get; set; } = false;

    public bool StopOnRevitShutdown { get; set; } = true;

    public bool AllowStopAnyInstance { get; set; } = false;

    public int Port { get; set; } = 8765;

    public string ServerExecutablePath { get; set; } = string.Empty;

    public string ServerArguments { get; set; } = string.Empty;

    public string DefaultFamilyLibraryPath { get; set; } = string.Empty;

    public string DefaultFamilyUpgradeOutputPath { get; set; } = string.Empty;

    public string HelpUrl { get; set; } =
        "https://help.autodesk.com/view/RVT/2027/ENU/?guid=GUID-620ECD98-53F7-47F1-B700-EEE84F15EBF7";
}
