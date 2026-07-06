using System.Reflection;
using Autodesk.Revit.UI;
using RevitPublicMCPBridge.Services;
using RevitPublicMCPBridge.Utilities;

namespace RevitPublicMCPBridge;

public sealed class RevitPublicMCPBridge : IExternalApplication
{
    private const string TabName = "MCP";
    private const string PanelName = "Public MCP Server";

    public Result OnStartup(UIControlledApplication application)
    {
        try
        {
            BridgeContext.Initialize(application.ControlledApplication.VersionNumber);
            BridgeContext.Instance.StartGateway();
            CreateRibbon(application);
            TryAutoStart();
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            BridgeContext.Logger?.Error("Startup failed.", ex);
            TaskDialog.Show(
                "Revit Public MCP Bridge",
                $"Failed to initialize RevitPublicMCPBridge.\n\n{ex.Message}");
            return Result.Failed;
        }
    }

    public Result OnShutdown(UIControlledApplication application)
    {
        try
        {
            BridgeContext.Instance?.HandleRevitShutdown();
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            BridgeContext.Logger?.Error("Shutdown failed.", ex);
            return Result.Failed;
        }
    }

    private void TryAutoStart()
    {
        var settings = BridgeContext.Instance.SettingsStore.Current;
        if (!settings.AutoStartOnLaunch)
        {
            return;
        }

        var startResult = BridgeContext.Instance.Controller.StartServer();
        if (!startResult.Success)
        {
            BridgeContext.Logger.Warn($"Auto-start failed: {startResult.Message}");
        }
    }

    private void CreateRibbon(UIControlledApplication app)
    {
        try
        {
            app.CreateRibbonTab(TabName);
        }
        catch
        {
            // Tab already exists, safe to continue.
        }

        var panel = app.CreateRibbonPanel(TabName, PanelName);
        var assemblyPath = Assembly.GetExecutingAssembly().Location;

        AddButton(
            panel,
            assemblyPath,
            "RevitPublicMCPBridge.Commands.StartMCPCommand",
            "Link / Start",
            "Check MCP link status. Starts server only if process control is enabled in Settings.",
            IconFactory.CreatePlayIcon());

        AddButton(
            panel,
            assemblyPath,
            "RevitPublicMCPBridge.Commands.StopMCPCommand",
            "Stop (Advanced)",
            "Stop MCP only when process control is enabled in Settings.",
            IconFactory.CreateStopIcon());

        AddButton(
            panel,
            assemblyPath,
            "RevitPublicMCPBridge.Commands.StatusCommand",
            "Status",
            "Show MCP server status, endpoint, and runtime details.",
            IconFactory.CreateStatusIcon());

        AddButton(
            panel,
            assemblyPath,
            "RevitPublicMCPBridge.Commands.CopyConfigCommand",
            "Copy Config",
            "Copy MCP connection details/instructions for Cursor or Claude.",
            IconFactory.CreateCopyIcon());

        AddButton(
            panel,
            assemblyPath,
            "RevitPublicMCPBridge.Commands.OpenSettingsCommand",
            "Settings",
            "Open bridge settings (auto-start, port, executable path).",
            IconFactory.CreateSettingsIcon());

        AddButton(
            panel,
            assemblyPath,
            "RevitPublicMCPBridge.Commands.OpenDocsCommand",
            "Help / Docs",
            "Open Autodesk docs/help URL for Public MCP setup.",
            IconFactory.CreateHelpIcon());
    }

    private static void AddButton(
        RibbonPanel panel,
        string assemblyPath,
        string commandClass,
        string text,
        string tooltip,
        System.Windows.Media.ImageSource icon)
    {
        var data = new PushButtonData(text, text, assemblyPath, commandClass)
        {
            ToolTip = tooltip,
            LargeImage = icon,
            Image = icon,
        };

        panel.AddItem(data);
    }
}
