using System.Diagnostics;

namespace SnapTrace.Core.Attributes;

/// <summary>
/// Forces a deep copy of a reference type to be stored in the trace buffer. This is useful
/// for logging the state of an object which may otherwise be victim to mutation. The class
/// is copied to bytecode and later deserialized. It is highly recommended to specify SnapTraceDeep
/// for all objects.
/// <para>
/// When applied to a method, it saves the whole method (return and params) using a deep copy.
/// When applied to a parameter or return value, it saves only those specific values.
/// </para>
/// </summary>
[Conditional("SNAPTRACE")]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
public class SnapTraceDeepAttribute : Attribute { }
