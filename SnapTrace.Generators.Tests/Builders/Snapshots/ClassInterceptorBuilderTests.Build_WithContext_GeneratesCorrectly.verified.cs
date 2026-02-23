internal static class MyClass_SnapTrace
{
    [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.StaticMethod, Name = "Record")]
    extern static void CallRecord_SnapTrace(global::SnapTrace.SnapTracer? target, string method, object? data, object? context, global::SnapTrace.SnapStatus status);

    [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Field, Name = "_balance")]
    extern static ref double Get__balance_SnapTrace(global::MyNamespace.MyClass @this);

    [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Field, Name = "_name")]
    extern static ref string Get__name_SnapTrace(global::MyNamespace.MyClass @this);

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static object? GetClassContext_SnapTrace(global::MyNamespace.MyClass @this)
    {
        return new { _balance = (object?)Get__balance_SnapTrace(@this), _name = (object?)Get__name_SnapTrace(@this) };
    }
}
