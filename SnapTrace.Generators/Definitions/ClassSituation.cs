using System;

namespace SnapTrace.Generators.Definitions;

[Flags]
public enum ClassSituation
{
    None = 0,
    Static = 1 << 0,      // Class is static (No 'this' parameter at all)
    Unsafe = 1 << 1,      // Class relies on unsafe context
    IsStruct = 1 << 2,    // Requires 'this ref' in the interceptor signature
    IsRefStruct = 1 << 3, // Requires 'this ref' AND prevents async interceptors
}
