#nullable enable
using global::SnapTrace;

namespace SnapTrace.Generated.TestApp
{
    internal static class MyService_SnapTrace
    {
        [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.StaticMethod, Name = "Record")]
        extern static void CallRecord_SnapTrace(global::SnapTrace.SnapTraceObserver? target, string method, object? data, object? context, global::SnapTrace.Runtime.Models.SnapStatus status);

        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static object? GetClassContext_SnapTrace(global::TestApp.MyService @this)
        {
            return null;
        }

        [global::System.Runtime.CompilerServices.InterceptsLocationAttribute(1, "NBaqmekbRfBuYwHqUY3Alk8BAABDOlxUZXN0c1xUZXN0UHJvamVjdC5jcw==")]
        [global::System.Runtime.CompilerServices.InterceptsLocationAttribute(1, "NBaqmekbRfBuYwHqUY3AlnQBAABDOlxUZXN0c1xUZXN0UHJvamVjdC5jcw==")]
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void DoWork_SnapTrace_void_string(this global::TestApp.MyService @this, string input)
        {
            object[] data = new object[] { /* input */ ((object)input)?.ToString() ?? "null" };

            var contextBefore = GetClassContext_SnapTrace(@this);
            CallRecord_SnapTrace(null!, "DoWork", data, contextBefore, global::SnapTrace.Runtime.Models.SnapStatus.Call);

            @this.DoWork(input);

            var contextAfter = GetClassContext_SnapTrace(@this);
            CallRecord_SnapTrace(null!, "DoWork", null, contextAfter, global::SnapTrace.Runtime.Models.SnapStatus.Return);
        }
    }
}
