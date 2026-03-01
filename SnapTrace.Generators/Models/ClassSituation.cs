using System;

namespace SnapTrace.Generators.Models;

[Flags]
public enum ClassSituation
{
    None = 0,
    Static = 1 << 0,
    Unsafe = 1 << 1,
    IsStruct = 1 << 2,
    IsRefStruct = 1 << 3,
}
