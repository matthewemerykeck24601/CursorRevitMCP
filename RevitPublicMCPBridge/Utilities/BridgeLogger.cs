using System.Diagnostics;
using System.IO;

namespace RevitPublicMCPBridge.Utilities;

public sealed class BridgeLogger
{
    private readonly string _path;

    public BridgeLogger(string path)
    {
        _path = path;
    }

    public void Info(string message) => Write("INFO", message, null);

    public void Warn(string message) => Write("WARN", message, null);

    public void Error(string message, Exception? ex) => Write("ERROR", message, ex);

    private void Write(string level, string message, Exception? ex)
    {
        try
        {
            var folder = Path.GetDirectoryName(_path);
            if (!string.IsNullOrWhiteSpace(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var body = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
            if (ex is not null)
            {
                body += $"{Environment.NewLine}{ex}";
            }

            File.AppendAllText(_path, body + Environment.NewLine);
            Debug.WriteLine(body);
        }
        catch
        {
            // Logging must never break command flow.
        }
    }
}
