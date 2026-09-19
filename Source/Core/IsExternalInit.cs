// Polyfill for IsExternalInit on .NET Standard 2.1 and older targets
// Enables C# 9+ init-only properties, records, etc.
using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit
    {
    }
}