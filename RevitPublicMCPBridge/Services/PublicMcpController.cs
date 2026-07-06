using RevitPublicMCPBridge.Settings;
using RevitPublicMCPBridge.Utilities;

namespace RevitPublicMCPBridge.Services;

public sealed class PublicMcpController
{
    private readonly SettingsStore _settingsStore;
    private readonly BridgeLogger _logger;
    private readonly ReflectionMcpClient _reflectionClient;
    private readonly ProcessMcpClient _processClient;

    public PublicMcpController(SettingsStore settingsStore, BridgeLogger logger)
    {
        _settingsStore = settingsStore;
        _logger = logger;
        _reflectionClient = new ReflectionMcpClient();
        _processClient = new ProcessMcpClient(settingsStore, logger);
    }

    public McpCommandResult StartServer()
    {
        try
        {
            var settings = _settingsStore.Current;
            if (!settings.EnableProcessControl)
            {
                var status = GetStatus();
                if (status.IsRunning)
                {
                    return McpCommandResult.Ok(
                        $"Public MCP is already available for Cursor/CLI.\nEndpoint: {status.Endpoint}\n\n" +
                        "Process control is disabled (link-only mode).");
                }

                return McpCommandResult.Ok(
                    $"Bridge is in link-only mode.\nExpected endpoint: {status.Endpoint}\n\n" +
                    "Use Autodesk's official MCP controls to start the server, or enable 'Allow bridge to start/stop MCP process' in Settings.");
            }

            if (_reflectionClient.TryStart(out var details))
            {
                var status = GetStatus();
                return McpCommandResult.Ok(
                    $"Public MCP Server started.\n{status.Endpoint}\n\nControl mode: Reflection\n{details}");
            }

            var processStart = _processClient.Start();
            if (processStart.Success)
            {
                return processStart;
            }

            return McpCommandResult.Fail($"{details}\n\n{processStart.Message}");
        }
        catch (Exception ex)
        {
            _logger.Error("StartServer failed.", ex);
            return McpCommandResult.Fail(ex.GetBaseException().Message);
        }
    }

    public McpCommandResult StopServer(bool? forceStopAny = null)
    {
        try
        {
            if (!_settingsStore.Current.EnableProcessControl)
            {
                return McpCommandResult.Ok(
                    "Process control is disabled (link-only mode). Nothing to stop from this bridge.");
            }

            if (_reflectionClient.TryStop(out var details))
            {
                return McpCommandResult.Ok($"Public MCP Server stopped.\n\n{details}");
            }

            var forceStop = forceStopAny ?? _settingsStore.Current.AllowStopAnyInstance;
            var processStop = _processClient.Stop(forceStop);
            if (processStop.Success)
            {
                return processStop;
            }

            return McpCommandResult.Fail($"{details}\n\n{processStop.Message}");
        }
        catch (Exception ex)
        {
            _logger.Error("StopServer failed.", ex);
            return McpCommandResult.Fail(ex.GetBaseException().Message);
        }
    }

    public McpStatus GetStatus()
    {
        var settings = _settingsStore.Current;
        var port = settings.Port;
        var endpoint = $"ws://localhost:{port}";

        var reflectionRunning = _reflectionClient.TryIsRunning();
        if (reflectionRunning.HasValue)
        {
            return new McpStatus
            {
                IsRunning = reflectionRunning.Value,
                Port = port,
                Endpoint = endpoint,
                ControlMode = "Reflection",
                ExecutablePath = _processClient.ResolveExecutablePath(),
                Details = $"Reflection bound type: {_reflectionClient.BoundType}",
            };
        }

        var running = _processClient.IsRunning();
        var pids = _processClient.RunningPids();
        var portOpen = _processClient.IsPortOpen(port);

        return new McpStatus
        {
            IsRunning = running,
            Port = port,
            Endpoint = endpoint,
            ControlMode = "Process",
            ExecutablePath = _processClient.ResolveExecutablePath(),
            Details = $"PID(s): {(pids.Length == 0 ? "none" : string.Join(", ", pids))}; Port open: {portOpen}",
        };
    }
}
