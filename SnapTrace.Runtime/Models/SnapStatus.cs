namespace SnapTrace.Runtime.Models;

/// <summary>
/// An marker for SnapEntry, indicating what type it is
/// </summary>
public enum SnapStatus : byte
{
    /// <summary>
    /// Indicates a method call entry point.
    /// </summary>
    Call = 0,

    /// <summary>
    /// Indicates a method return.
    /// </summary>
    Return = 1,

    /// <summary>
    /// Indicates an error or exception.
    /// </summary>
    Error = 2
}
