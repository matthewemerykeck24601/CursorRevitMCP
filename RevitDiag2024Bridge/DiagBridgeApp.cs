using System;
using System.IO;
using Autodesk.Revit.UI;
using RevitDiag2024Bridge.Services;
using RevitDiag2024Bridge.Utilities;

namespace RevitDiag2024Bridge;

// Entry point for the Revit 2024 read-only diagnostic bridge. On startup it creates the
// request dispatcher (an ExternalEvent bound to Revit's API thread) and starts the HTTP
// gateway on port 14001. There is no ribbon and no write capability — the bridge exists
// purely to let an MCP client observe a live Revit 2024 session.
public sealed class DiagBridgeApp : IExternalApplication
{
    // Port 14001 — deliberately distinct from the Revit 2025 bridge (14000).
    private const int GatewayPort = 14001;

    private RevitApiGatewayService? _gateway;
    private BridgeLogger? _logger;

    public Result OnStartup(UIControlledApplication application)
    {
        try
        {
            var logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RevitDiag2024Bridge",
                "bridge.log");
            _logger = new BridgeLogger(logPath);

            var dispatcher = new RevitRequestDispatcher();
            _gateway = new RevitApiGatewayService(_logger, dispatcher, GatewayPort);
            _gateway.Start();

            _logger.Info($"RevitDiag2024 diagnostic bridge started (read-only) on port {GatewayPort}.");
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger?.Error("Startup failed.", ex);
            TaskDialog.Show(
                "Revit Diag 2024 Bridge",
                $"Failed to start the read-only diagnostic bridge.\n\n{ex.Message}");
            return Result.Failed;
        }
    }

    public Result OnShutdown(UIControlledApplication application)
    {
        try
        {
            _gateway?.Stop();
            _logger?.Info("RevitDiag2024 diagnostic bridge stopped.");
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger?.Error("Shutdown failed.", ex);
            return Result.Failed;
        }
    }
}
