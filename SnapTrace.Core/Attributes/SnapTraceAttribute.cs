using System.Diagnostics;

namespace SnapTrace.Core.Attributes;

/// <summary>
/// Opts a class or method into SnapTrace recording.
/// <para>
/// When applied to a class, all public methods are traced by default.
/// When applied to a method, it explicitly enables tracing (useful for private/internal methods).
/// </para>
/// </summary>
[Conditional("SNAPTRACE")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SnapTraceAttribute : Attribute { }
