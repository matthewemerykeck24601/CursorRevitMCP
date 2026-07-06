using System.Text;
using System.IO;

namespace InventorMCPBridge.Utilities;

public sealed class BridgeLogger
{
    private readonly string _logPath;
    private readonly object _sync = new();

    public BridgeLogger(string logPath)
    {
        _logPath = logPath;
    }

    public void Info(string message) => Write("INFO", message);

    public void Warn(string message) => Write("WARN", message);

    public void Error(string message, Exception? ex = null)
    {
        if (ex is null)
        {
            Write("ERROR", message);
            return;
        }

        Write("ERROR", $"{message}{Environment.NewLine}{ex}");
    }

    private void Write(string level, string message)
    {
        var folder = Path.GetDirectoryName(_logPath);
        if (!string.IsNullOrWhiteSpace(folder) && !Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var line = new StringBuilder()
            .Append('[').Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append("] ")
            .Append(level).Append(' ')
            .Append(message)
            .AppendLine()
            .ToString();

        lock (_sync)
        {
            File.AppendAllText(_logPath, line);
        }
    }
}
