namespace SnapTrace.Core.Runtime;

/// <summary>
/// An marker for SnapEntry, indicating what type it is
/// </summary>
internal enum SnapStatus : byte
{
    Call = 0,
    Return = 1,
    Error = 2
}
