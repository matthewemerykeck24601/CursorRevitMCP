namespace RevitPublicMCPBridge.Services;

public sealed class McpStatus
{
    public bool IsRunning { get; init; }

    public int Port { get; init; }

    public string Endpoint { get; init; } = string.Empty;

    public string ControlMode { get; init; } = "Unknown";

    public string ExecutablePath { get; init; } = string.Empty;

    public string Details { get; init; } = string.Empty;
}

public sealed class McpCommandResult
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public static McpCommandResult Ok(string message) => new() { Success = true, Message = message };

    public static McpCommandResult Fail(string message) => new() { Success = false, Message = message };
}
