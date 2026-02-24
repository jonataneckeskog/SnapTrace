#nullable enable
using global::SnapTrace;

namespace SnapTrace.Generated.TestApp
{
    internal static class UserService_SnapTrace
    {
        [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.StaticMethod, Name = "Record")]
        extern static void CallRecord_SnapTrace(global::SnapTrace.SnapTraceObserver? target, string method, object? data, object? context, global::SnapTrace.Runtime.Models.SnapStatus status);

        [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Field, Name = "_userId")]
        extern static ref string Get__userId_SnapTrace(global::TestApp.UserService @this);

        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static object? GetClassContext_SnapTrace(global::TestApp.UserService @this)
        {
            return new { _userId = (object?)Get__userId_SnapTrace(@this) };
        }

        [global::System.Runtime.CompilerServices.InterceptsLocationAttribute(1, "qP0q+OA33E9FyZC82L1wP0MBAABDOlxUZXN0c1xUZXN0UHJvamVjdC5jcw==")]
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void UpdateUser_SnapTrace_void_string(this global::TestApp.UserService @this, string name)
        {
            object[] data = new object[] { /* name */ ((object)name)?.ToString() ?? "null" };

            var contextBefore = GetClassContext_SnapTrace(@this);
            CallRecord_SnapTrace(null!, "UpdateUser", data, contextBefore, global::SnapTrace.Runtime.Models.SnapStatus.Call);

            @this.UpdateUser(name);

            var contextAfter = GetClassContext_SnapTrace(@this);
            CallRecord_SnapTrace(null!, "UpdateUser", null, contextAfter, global::SnapTrace.Runtime.Models.SnapStatus.Return);
        }
    }
}
