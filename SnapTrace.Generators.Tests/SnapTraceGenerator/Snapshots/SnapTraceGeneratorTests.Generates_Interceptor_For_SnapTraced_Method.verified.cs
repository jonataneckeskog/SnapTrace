using global::SnapTrace;

namespace TestApp
{
    internal static class MyService_SnapTrace
    {
        [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.StaticMethod, Name = "Record")]
        extern static void CallRecord_SnapTrace(global::SnapTrace.SnapTracer? target, string method, object? data, object? context, global::SnapTrace.SnapStatus status);

        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static object? GetClassContext_SnapTrace(global::TestApp.MyService @this)
        {
            return null;
        }

        [global::System.Runtime.CompilerServices.InterceptsLocation(@"C:\\Tests\\TestProject.cs", 28, 21)]
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void DoWork_SnapTrace_void_string(global::TestApp.MyService @this, string input)
        {
            object? data = new object[] { /* input */ input };
            var context = GetClassContext_SnapTrace(@this);

            CallRecord_SnapTrace(null!, "DoWork", data, context, global::SnapTrace.SnapStatus.Call);
            @this.DoWork(input);
            CallRecord_SnapTrace(null!, "DoWork", null, context, global::SnapTrace.SnapStatus.Return);
        }
    }
}
