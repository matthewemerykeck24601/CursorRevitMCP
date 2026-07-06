using System.Reflection;

namespace RevitPublicMCPBridge.Services;

/// <summary>
/// Best-effort reflection adapter in case Autodesk exposes MCP control APIs in-process.
/// Safe fallback to process control is always used if this adapter cannot bind.
/// </summary>
public sealed class ReflectionMcpClient
{
    private object? _instance;
    private MethodInfo? _startMethod;
    private MethodInfo? _stopMethod;
    private MethodInfo? _statusMethod;
    private string _boundType = string.Empty;

    public bool IsBound => _instance is not null;

    public string BoundType => _boundType;

    public bool TryBind()
    {
        if (IsBound)
        {
            return true;
        }

        var candidateTypeNames = new[]
        {
            "Autodesk.Revit.Mcp.PublicMcpServer",
            "Autodesk.Revit.Mcp.Server.PublicMcpServer",
            "Autodesk.Revit.Mcp.PublicMcpBridge",
        };

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type? hit = null;
            foreach (var name in candidateTypeNames)
            {
                hit = asm.GetType(name, throwOnError: false, ignoreCase: false);
                if (hit is not null)
                {
                    break;
                }
            }

            if (hit is null)
            {
                continue;
            }

            var methods = hit.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            _startMethod = methods.FirstOrDefault(m => m.Name is "Start" or "StartServer");
            _stopMethod = methods.FirstOrDefault(m => m.Name is "Stop" or "StopServer");
            _statusMethod = methods.FirstOrDefault(m => m.Name is "IsRunning" or "GetStatus");
            if (_startMethod is null || _stopMethod is null)
            {
                continue;
            }

            _instance = _startMethod.IsStatic ? null : Activator.CreateInstance(hit);
            _boundType = hit.FullName ?? hit.Name;
            return true;
        }

        return false;
    }

    public bool TryStart(out string details)
    {
        details = "Reflection strategy unavailable.";
        if (!TryBind() || _startMethod is null)
        {
            return false;
        }

        try
        {
            _startMethod.Invoke(_instance, null);
            details = $"Started via reflection type {_boundType}.";
            return true;
        }
        catch (Exception ex)
        {
            details = $"Reflection start failed: {ex.GetBaseException().Message}";
            return false;
        }
    }

    public bool TryStop(out string details)
    {
        details = "Reflection strategy unavailable.";
        if (!TryBind() || _stopMethod is null)
        {
            return false;
        }

        try
        {
            _stopMethod.Invoke(_instance, null);
            details = $"Stopped via reflection type {_boundType}.";
            return true;
        }
        catch (Exception ex)
        {
            details = $"Reflection stop failed: {ex.GetBaseException().Message}";
            return false;
        }
    }

    public bool? TryIsRunning()
    {
        if (!TryBind() || _statusMethod is null)
        {
            return null;
        }

        try
        {
            var raw = _statusMethod.Invoke(_instance, null);
            if (raw is bool b)
            {
                return b;
            }
        }
        catch
        {
            // Ignore and fall back.
        }

        return null;
    }
}
