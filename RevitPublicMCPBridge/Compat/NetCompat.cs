using System;
using System.IO;

namespace RevitPublicMCPBridge;

/// <summary>
/// Small BCL polyfills so one codebase compiles on both the .NET 8 (Revit 2025/2026) and
/// .NET Framework 4.8 (Revit 2024) targets.
/// </summary>
internal static class NetCompat
{
    /// <summary>
    /// Returns a relative path from <paramref name="relativeTo"/> to <paramref name="path"/>.
    /// Uses <see cref="Path.GetRelativePath"/> on net5+; falls back to a Uri-based computation
    /// on net48 where that API is unavailable.
    /// </summary>
    public static string GetRelativePath(string relativeTo, string path)
    {
#if NET5_0_OR_GREATER
        return Path.GetRelativePath(relativeTo, path);
#else
        if (string.IsNullOrEmpty(relativeTo))
        {
            return path;
        }

        var fromFull = AppendDirectorySeparator(Path.GetFullPath(relativeTo));
        var toFull = Path.GetFullPath(path);

        // Different volumes (e.g. C:\ vs D:\) have no relative path; return the absolute target.
        if (!string.Equals(Path.GetPathRoot(fromFull), Path.GetPathRoot(toFull),
                StringComparison.OrdinalIgnoreCase))
        {
            return toFull;
        }

        var fromUri = new Uri(fromFull);
        var toUri = new Uri(toFull);
        var relativeUri = fromUri.MakeRelativeUri(toUri);
        var relative = Uri.UnescapeDataString(relativeUri.ToString());
        return relative.Replace('/', Path.DirectorySeparatorChar);
#endif
    }

#if !NET5_0_OR_GREATER
    private static string AppendDirectorySeparator(string dir)
    {
        return dir.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
            ? dir
            : dir + Path.DirectorySeparatorChar;
    }
#endif
}
