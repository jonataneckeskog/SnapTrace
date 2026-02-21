using System;

namespace SnapTrace.Generators.Definitions;

[Flags]
public enum MethodSituation
{
    None = 0,
    Static = 1 << 0,
    Async = 1 << 1,
    Unsafe = 1 << 2,
    Generic = 1 << 3,
    ReturnsRef = 1 << 4,
    ReturnsRefReadonly = 1 << 5
}
