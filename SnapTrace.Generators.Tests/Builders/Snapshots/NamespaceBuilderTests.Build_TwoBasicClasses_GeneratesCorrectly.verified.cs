namespace MyNamespace
{
    internal static class MyClass1_SnapTrace
    {
        [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.StaticMethod, Name = "Record")]
        extern static void CallRecord_SnapTrace(global::SnapTrace.SnapTracer? target, string method, object? data, object? context, global::SnapTrace.SnapStatus status);

        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static object? GetClassContext_SnapTrace(global::MyNamespace.MyClass1 @this)
        {
            return null;
        }
    }
    
    internal static class MyClass2_SnapTrace
    {
        [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.StaticMethod, Name = "Record")]
        extern static void CallRecord_SnapTrace(global::SnapTrace.SnapTracer? target, string method, object? data, object? context, global::SnapTrace.SnapStatus status);

        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static object? GetClassContext_SnapTrace(global::MyNamespace.MyClass2 @this)
        {
            return null;
        }
    }
}
