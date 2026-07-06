using System.IO;
using System.Windows.Threading;
using InventorMCPBridge.Settings;
using InventorMCPBridge.Utilities;
using InventorApplication = Inventor.Application;

namespace InventorMCPBridge.Services;

public sealed class BridgeContext
{
    private static BridgeContext? _instance;

    public static BridgeContext Instance =>
        _instance ?? throw new InvalidOperationException("Bridge context not initialized.");

    public static BridgeLogger Logger => Instance.LoggerService;

    public static void Initialize(
        InventorApplication app,
        SynchronizationContext? uiContext,
        Dispatcher? uiDispatcher)
    {
        if (_instance is not null)
        {
            return;
        }

        var appData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
        var root = System.IO.Path.Combine(appData, "InventorMCPBridge");
        var settingsPath = System.IO.Path.Combine(root, "settings.json");
        var logPath = System.IO.Path.Combine(root, "bridge.log");

        var settingsStore = new SettingsStore(settingsPath);
        settingsStore.Load();

        var logger = new BridgeLogger(logPath);
        var dispatcher = new InventorRequestDispatcher();
        dispatcher.BindUiContext(uiContext);
        dispatcher.BindUiDispatcher(uiDispatcher);

        var iLogicService = new ILogicExecutionServiceImpl(app);
        var replicationService = new ParameterReplicationService();
        var gateway = new InventorApiGatewayService(
            settingsStore,
            logger,
            dispatcher,
            iLogicService,
            replicationService);

        _instance = new BridgeContext(
            app,
            settingsStore,
            logger,
            dispatcher,
            iLogicService,
            replicationService,
            gateway);
    }

    private BridgeContext(
        InventorApplication app,
        SettingsStore settingsStore,
        BridgeLogger logger,
        InventorRequestDispatcher dispatcher,
        ILogicExecutionService iLogicExecutionService,
        ParameterReplicationService parameterReplicationService,
        InventorApiGatewayService gateway)
    {
        App = app;
        SettingsStore = settingsStore;
        LoggerService = logger;
        Dispatcher = dispatcher;
        ILogicExecutionService = iLogicExecutionService;
        ParameterReplicationService = parameterReplicationService;
        Gateway = gateway;
    }

    public InventorApplication App { get; }

    public SettingsStore SettingsStore { get; }

    public BridgeLogger LoggerService { get; }

    public InventorRequestDispatcher Dispatcher { get; }

    public ILogicExecutionService ILogicExecutionService { get; }

    public ParameterReplicationService ParameterReplicationService { get; }

    public InventorApiGatewayService Gateway { get; }

    public void StartGateway()
    {
        Gateway.Start();
    }

    public void StopGateway()
    {
        Gateway.Stop();
    }
}
