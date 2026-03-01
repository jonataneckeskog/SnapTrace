#nullable enable
using global::SnapTrace;

namespace SnapTrace.Generated.MyNamespace
{
    internal static class MyClass_SnapTrace
    {
        [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.StaticMethod, Name = "Record")]
        extern static void CallRecord_SnapTrace(global::SnapTrace.SnapTraceObserver? target, string method, object? data, object? context, global::SnapTrace.Runtime.Models.SnapStatus status);

        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static object? GetClassContext_SnapTrace()
        {
            return null;
        }

        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void MyTestMethod_SnapTrace_void(this global::MyNamespace.MyClass @this)
        {
            object?[]? data = null;

            var contextBefore = GetClassContext_SnapTrace();
            CallRecord_SnapTrace(null!, "MyTestMethod", data, contextBefore, global::SnapTrace.Runtime.Models.SnapStatus.Call);

            @this.MyTestMethod();

            var contextAfter = GetClassContext_SnapTrace();
            CallRecord_SnapTrace(null!, "MyTestMethod", null, contextAfter, global::SnapTrace.Runtime.Models.SnapStatus.Return);
        }
    }
}
