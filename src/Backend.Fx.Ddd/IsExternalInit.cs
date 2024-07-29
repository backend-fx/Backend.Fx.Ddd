using JetBrains.Annotations;

#if NETSTANDARD
// ReSharper disable once CheckNamespace
// The IsExternalInit type is only included in the net5.0 (and future) target frameworks. When compiling
// against older target frameworks you will need to manually define this type.
namespace System.Runtime.CompilerServices;

[UsedImplicitly]
internal static class IsExternalInit {}
#endif