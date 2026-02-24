internal static class MyClass_SnapTrace
{
    [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.StaticMethod, Name = "Record")]
    extern static void CallRecord_SnapTrace(global::SnapTrace.SnapTraceObserver? target, string method, object? data, object? context, global::SnapTrace.Runtime.Models.SnapStatus status);

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static object? GetClassContext_SnapTrace(global::MyNamespace.MyClass @this)
    {
        return null;
    }

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void MyMethod_SnapTrace_void(this global::MyNamespace.MyClass @this)
    {
        object? data = null;

        var contextBefore = GetClassContext_SnapTrace(@this);
        CallRecord_SnapTrace(null!, "MyMethod", data, contextBefore, global::SnapTrace.Runtime.Models.SnapStatus.Call);

        @this.MyMethod();

        var contextAfter = GetClassContext_SnapTrace(@this);
        CallRecord_SnapTrace(null!, "MyMethod", null, contextAfter, global::SnapTrace.Runtime.Models.SnapStatus.Return);
    }
}
