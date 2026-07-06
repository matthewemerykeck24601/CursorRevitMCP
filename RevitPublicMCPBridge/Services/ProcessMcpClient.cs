using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using RevitPublicMCPBridge.Settings;
using RevitPublicMCPBridge.Utilities;

namespace RevitPublicMCPBridge.Services;

public sealed class ProcessMcpClient
{
    private readonly SettingsStore _settingsStore;
    private readonly BridgeLogger _logger;
    private int? _startedPid;

    public ProcessMcpClient(SettingsStore settingsStore, BridgeLogger logger)
    {
        _settingsStore = settingsStore;
        _logger = logger;
    }

    public string ResolveExecutablePath()
    {
        var configured = _settingsStore.Current.ServerExecutablePath?.Trim();
        if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured))
        {
            return configured;
        }

        var currentRevitExe = Process.GetCurrentProcess().MainModule?.FileName;
        var currentRevitDir = string.IsNullOrWhiteSpace(currentRevitExe)
            ? null
            : Path.GetDirectoryName(currentRevitExe);

        var defaults = new[]
        {
            currentRevitDir is null ? string.Empty : Path.Combine(currentRevitDir, "Autodesk.RevitMcpServer.Stdio.exe"),
            currentRevitDir is null ? string.Empty : Path.Combine(currentRevitDir, "AddIns", "Autodesk.RevitMcpServer.Stdio.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Autodesk", "Revit 2025", "Autodesk.RevitMcpServer.Stdio.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Autodesk", "Revit 2025", "AddIns", "Autodesk.RevitMcpServer.Stdio.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Autodesk", "Revit 2026", "Autodesk.RevitMcpServer.Stdio.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Autodesk", "Revit 2026", "AddIns", "Autodesk.RevitMcpServer.Stdio.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Autodesk", "Revit 2027", "Autodesk.RevitMcpServer.Stdio.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Autodesk", "Revit 2027", "AddIns", "Autodesk.RevitMcpServer.Stdio.exe"),
        };

        return defaults.FirstOrDefault(File.Exists) ?? defaults[0];
    }

    public bool IsRunning()
    {
        var processes = GetCandidateProcesses();
        return processes.Count > 0;
    }

    public int[] RunningPids()
    {
        return GetCandidateProcesses().Select(p => p.Id).ToArray();
    }

    public bool IsPortOpen(int port, int timeoutMs = 200)
    {
        try
        {
            using var client = new TcpClient();
            var task = client.ConnectAsync("127.0.0.1", port);
            return task.Wait(timeoutMs) && client.Connected;
        }
        catch
        {
            return false;
        }
    }

    public McpCommandResult Start()
    {
        if (IsRunning())
        {
            return McpCommandResult.Ok("Public MCP Server is already running.");
        }

        var exe = ResolveExecutablePath();
        if (!File.Exists(exe))
        {
            return McpCommandResult.Fail(
                $"Public MCP executable not found.\nExpected path:\n{exe}\n\nSet the path in Settings.");
        }

        var settings = _settingsStore.Current;
        var args = BuildArgs(settings);

        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = exe,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(exe) ?? Environment.CurrentDirectory,
            });

            if (process is null)
            {
                return McpCommandResult.Fail("Process start returned null.");
            }

            _startedPid = process.Id;
            _logger.Info($"Started MCP process. PID={process.Id}, exe={exe}, args={args}");
            return McpCommandResult.Ok(
                $"Public MCP Server started.\nPID: {process.Id}\nEndpoint: ws://localhost:{settings.Port}");
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to start process.", ex);
            return McpCommandResult.Fail($"Failed to start process: {ex.GetBaseException().Message}");
        }
    }

    public McpCommandResult Stop(bool forceStopAny)
    {
        var targets = GetCandidateProcesses();
        if (targets.Count == 0)
        {
            return McpCommandResult.Ok("Public MCP Server is not running.");
        }

        if (!forceStopAny && _startedPid.HasValue)
        {
            targets = targets.Where(p => p.Id == _startedPid.Value).ToList();
            if (targets.Count == 0)
            {
                return McpCommandResult.Fail(
                    "MCP appears to be running, but it was not started by this bridge instance. " +
                    "Enable 'Allow stop any instance' in Settings if you want to force stop.");
            }
        }

        var stopped = new List<int>();
        foreach (var p in targets)
        {
            try
            {
#if NET5_0_OR_GREATER
                p.Kill(entireProcessTree: true);
#else
                p.Kill();
#endif
                p.WaitForExit(2000);
                stopped.Add(p.Id);
            }
            catch (Exception ex)
            {
                _logger.Warn($"Could not stop PID={p.Id}: {ex.GetBaseException().Message}");
            }
        }

        if (stopped.Count == 0)
        {
            return McpCommandResult.Fail("Unable to stop MCP process.");
        }

        _logger.Info($"Stopped MCP PID(s): {string.Join(", ", stopped)}");
        return McpCommandResult.Ok($"Stopped MCP PID(s): {string.Join(", ", stopped)}");
    }

    private string BuildArgs(BridgeSettings settings)
    {
        var args = settings.ServerArguments?.Trim() ?? string.Empty;
        if (!args.Contains("--port", StringComparison.OrdinalIgnoreCase) && settings.Port > 0)
        {
            args = string.IsNullOrWhiteSpace(args) ? $"--port {settings.Port}" : $"{args} --port {settings.Port}";
        }

        return args;
    }

    private List<Process> GetCandidateProcesses()
    {
        var nameCandidates = new[]
        {
            "Autodesk.RevitMcpServer.Stdio",
            "RevitMcpServer",
        };

        var results = new List<Process>();
        foreach (var baseName in nameCandidates)
        {
            try
            {
                results.AddRange(Process.GetProcessesByName(baseName));
            }
            catch
            {
                // Ignore.
            }
        }

        return results
            .GroupBy(p => p.Id)
            .Select(g => g.First())
            .ToList();
    }
}
