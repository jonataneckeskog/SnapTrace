namespace SnapTrace.Generators;

internal static class AttributeDefinitions
{
    /// <summary>
    /// The attributes served by SnapTrace. All marked with "SNAPTRACE",
    /// meaning that the compilator can chose to ignore them if prompted.
    /// </summary>
    internal const string Definitions = @"
using System;
using System.Diagnostics;

namespace SnapTrace
{
    [Conditional(""SNAPTRACE"")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    internal sealed class SnapTraceAttribute : Attribute { }

    [Conditional(""SNAPTRACE"")]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
    internal class SnapTraceContextAttribute : Attribute { }

    [Conditional(""SNAPTRACE"")]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
    internal class SnapTraceIgnoreAttribute : Attribute { }

    [Conditional(""SNAPTRACE"")]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
    internal class SnapTraceDeepAttribute : Attribute { }
}
";
}