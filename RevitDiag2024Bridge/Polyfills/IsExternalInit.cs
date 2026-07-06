// net48 has no System.Runtime.CompilerServices.IsExternalInit, which the C# compiler
// requires to emit `init`-only setters (used throughout the gateway DTOs). This shim
// supplies it. Harmless on frameworks that already define the type (they won't, here).
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit
    {
    }
}
