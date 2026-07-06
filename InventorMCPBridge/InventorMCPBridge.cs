using System.Runtime.InteropServices;
using System.Windows.Threading;
using Inventor;
using InventorMCPBridge.Commands;
using InventorMCPBridge.Services;

namespace InventorMCPBridge;

[ComVisible(true)]
[Guid("6EB7BD5E-C89E-4ED2-8DA0-3E065D70AB11")]
public sealed class InventorMCPBridge : ApplicationAddInServer
{
    private const string AddInClientId = "{6EB7BD5E-C89E-4ED2-8DA0-3E065D70AB11}";
    private const string PanelInternalName = "mcpBridgeRibbonPanel";
    private Inventor.Application? _app;
    private readonly List<ButtonDefinition> _buttons = [];
    private readonly List<Action> _buttonHandlers = [];

    public object Automation => this;

    public void Activate(ApplicationAddInSite addInSiteObject, bool firstTime)
    {
        _app = addInSiteObject.Application;
        BridgeContext.Initialize(_app, SynchronizationContext.Current, Dispatcher.CurrentDispatcher);

        if (BridgeContext.Instance.SettingsStore.Current.AutoStartOnLaunch)
        {
            BridgeContext.Instance.StartGateway();
        }

        CreateUi();
    }

    public void Deactivate()
    {
        try
        {
            BridgeContext.Instance.StopGateway();
        }
        catch
        {
            // ignore
        }

        _buttons.Clear();
        _buttonHandlers.Clear();
        _app = null;
    }

    public void ExecuteCommand(int commandID)
    {
        // not used
    }

    private void CreateUi()
    {
        if (_app is null)
        {
            return;
        }

        var commandManager = _app.CommandManager;
        var bridgeLinkButton = AddButton(
            commandManager,
            "mcpBridgeLinkBtn",
            "Bridge Link",
            () => new BridgeLinkCommand().Execute());
        var startGatewayButton = AddButton(
            commandManager,
            "mcpStartGatewayBtn",
            "Start Gateway",
            () => new StartGatewayCommand().Execute());
        var stopGatewayButton = AddButton(
            commandManager,
            "mcpStopGatewayBtn",
            "Stop Gateway",
            () => new StopGatewayCommand().Execute());
        var statusButton = AddButton(
            commandManager,
            "mcpStatusBtn",
            "Status",
            () => new StatusCommand().Execute());
        var copyConfigButton = AddButton(
            commandManager,
            "mcpCopyConfigBtn",
            "Copy Config",
            () => new CopyConfigCommand().Execute());
        var settingsButton = AddButton(
            commandManager,
            "mcpSettingsBtn",
            "Settings",
            () => new OpenSettingsCommand().Execute());

        AddButtonsToRibbonPanels(
            bridgeLinkButton,
            startGatewayButton,
            stopGatewayButton,
            statusButton,
            copyConfigButton,
            settingsButton);
    }

    private ButtonDefinition AddButton(
        CommandManager commandManager,
        string internalName,
        string displayName,
        Action handler)
    {
        var controlDef = commandManager.ControlDefinitions.AddButtonDefinition(
            displayName,
            internalName,
            CommandTypesEnum.kShapeEditCmdType);

        controlDef.OnExecute += _ => handler();
        _buttons.Add(controlDef);
        _buttonHandlers.Add(handler);
        return controlDef;
    }

    private void AddButtonsToRibbonPanels(params ButtonDefinition[] buttons)
    {
        if (_app is null)
        {
            return;
        }

        string[] ribbonNames = ["Assembly", "Part", "ZeroDoc"];
        foreach (var ribbonName in ribbonNames)
        {
            try
            {
                var ribbon = _app.UserInterfaceManager.Ribbons[ribbonName];
                var toolsTab = ribbon.RibbonTabs["id_TabTools"];
                var panel = GetOrCreatePanel(toolsTab, $"{PanelInternalName}_{ribbonName}");
                foreach (var button in buttons)
                {
                    TryAddButton(panel, button);
                }
            }
            catch
            {
                // Ribbon may differ by environment/version; skip gracefully.
            }
        }
    }

    private RibbonPanel GetOrCreatePanel(RibbonTab tab, string panelName)
    {
        try
        {
            return tab.RibbonPanels[panelName];
        }
        catch
        {
            return tab.RibbonPanels.Add("MCP Bridge", panelName, AddInClientId);
        }
    }

    private static void TryAddButton(RibbonPanel panel, ButtonDefinition button)
    {
        try
        {
            panel.CommandControls.AddButton(button, true);
        }
        catch
        {
            // Button likely already exists in this panel.
        }
    }
}
