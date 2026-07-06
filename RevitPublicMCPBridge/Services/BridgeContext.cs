using System.IO;
using RevitPublicMCPBridge.Settings;
using RevitPublicMCPBridge.Utilities;

namespace RevitPublicMCPBridge.Services;

public sealed class BridgeContext
{
    private static BridgeContext? _instance;

    public static BridgeContext Instance =>
        _instance ?? throw new InvalidOperationException("BridgeContext is not initialized.");

    public static BridgeLogger Logger => Instance.LoggerInternal;

    private BridgeLogger LoggerInternal { get; }

    public SettingsStore SettingsStore { get; }

    public PublicMcpController Controller { get; }

    public RevitRequestDispatcher Dispatcher { get; }

    public RevitApiGatewayService GatewayService { get; }

    private BridgeContext(
        BridgeLogger logger,
        SettingsStore settingsStore,
        PublicMcpController controller,
        RevitRequestDispatcher dispatcher,
        RevitApiGatewayService gatewayService)
    {
        LoggerInternal = logger;
        SettingsStore = settingsStore;
        Controller = controller;
        Dispatcher = dispatcher;
        GatewayService = gatewayService;
    }

    public static void Initialize(string? revitVersionNumber = null)
    {
        if (_instance is not null)
        {
            return;
        }

        var baseFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RevitPublicMCPBridge");
        var revitYear = ResolveRevitYear(revitVersionNumber);
        var settingsPath = ResolveSettingsPath(baseFolder, revitYear);
        var logPath = Path.Combine(baseFolder, "bridge.log");

        var logger = new BridgeLogger(logPath);
        var settingsStore = new SettingsStore(settingsPath);
        settingsStore.Load();
        EnsureDefaultGatewayPort(settingsStore, revitYear);
        EnsureDefaultFamilyLibraryPath(settingsStore, revitYear);

        var controller = new PublicMcpController(settingsStore, logger);
        var dispatcher = new RevitRequestDispatcher();
        var gateway = new RevitApiGatewayService(settingsStore, logger, dispatcher);
        _instance = new BridgeContext(logger, settingsStore, controller, dispatcher, gateway);

        logger.Info($"Bridge context initialized (Revit {revitYear}, gateway settings: {settingsPath}).");
    }

    private static int ResolveRevitYear(string? revitVersionNumber)
    {
        if (!string.IsNullOrWhiteSpace(revitVersionNumber))
        {
            var digits = new string(revitVersionNumber.TakeWhile(char.IsDigit).ToArray());
            if (digits.Length >= 4 && int.TryParse(digits[..4], out var year) && year is >= 2019 and <= 2035)
            {
                return year;
            }
        }

        return 2026;
    }

    private static string ResolveSettingsPath(string baseFolder, int revitYear)
    {
        var yearPath = Path.Combine(baseFolder, $"settings-{revitYear}.json");
        if (File.Exists(yearPath))
        {
            return yearPath;
        }

        var legacyPath = Path.Combine(baseFolder, "settings.json");
        if (File.Exists(legacyPath) && revitYear == 2026)
        {
            File.Copy(legacyPath, yearPath);
            return yearPath;
        }

        return yearPath;
    }

    private static void EnsureDefaultGatewayPort(SettingsStore settingsStore, int revitYear)
    {
        var expectedPort = DefaultGatewayPort(revitYear);
        if (settingsStore.Current.LocalApiGatewayPort == expectedPort)
        {
            return;
        }

        // Only auto-assign when still on the global default (8766) or invalid.
        var current = settingsStore.Current.LocalApiGatewayPort;
        if (current is > 0 and < 65536 && current != 8766)
        {
            return;
        }

        var updated = settingsStore.Current;
        updated.LocalApiGatewayPort = expectedPort;
        settingsStore.Save(updated);
    }

    // Seed the family library path once (only when unset) so the write bridge resolves the
    // Metromont 2024 library out of the box. The Settings UI folder picker overrides this.
    private static void EnsureDefaultFamilyLibraryPath(SettingsStore settingsStore, int revitYear)
    {
        if (revitYear != 2024)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(settingsStore.Current.DefaultFamilyLibraryPath))
        {
            return;
        }

        const string metromont2024Library =
            @"D:\Autodesk\AutodeskCache\ACCDocs\Metromont\01 - Metromont Standards\Project Files\002 - Families\2024";
        if (!Directory.Exists(metromont2024Library))
        {
            return;
        }

        var updated = settingsStore.Current;
        updated.DefaultFamilyLibraryPath = metromont2024Library;
        settingsStore.Save(updated);
    }

    private static int DefaultGatewayPort(int revitYear) => revitYear switch
    {
        2024 => 8764,
        2025 => 8767,
        2026 => 8766,
        2027 => 8768,
        _ => 8766 + Math.Max(0, revitYear - 2026),
    };

    public void StartGateway()
    {
        GatewayService.Start();
    }

    public void HandleRevitShutdown()
    {
        GatewayService.Stop();
        if (SettingsStore.Current.StopOnRevitShutdown)
        {
            var result = Controller.StopServer(forceStopAny: false);
            LoggerInternal.Info($"Shutdown stop result: {result.Message}");
        }
    }
}
