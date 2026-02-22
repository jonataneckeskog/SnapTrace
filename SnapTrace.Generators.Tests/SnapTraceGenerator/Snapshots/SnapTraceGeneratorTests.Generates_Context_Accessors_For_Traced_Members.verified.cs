using global::SnapTrace;

namespace TestApp
{
    internal static class UserService_SnapTrace
    {
        [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.StaticMethod, Name = "Record")]
        extern static void CallRecord_SnapTrace(global::SnapTrace.SnapTracer? target, string method, object? data, object? context, global::SnapTrace.SnapStatus status);

        [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Field, Name = "_userId")]
        extern static ref string Get__userId_SnapTrace(global::TestApp.UserService @this);

        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static object? GetClassContext_SnapTrace(global::TestApp.UserService @this)
        {
            return new { _userId = (object?)Get__userId_SnapTrace(@this) };
        }

        [global::System.Runtime.CompilerServices.InterceptsLocation(@"C:\\Tests\\TestProject.cs", 20, 31)]
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void UpdateUser_SnapTrace_void_string(global::TestApp.UserService @this, string name)
        {
            object? data = new object[] { /* name */ name };
            var context = GetClassContext_SnapTrace(@this);

            CallRecord_SnapTrace(null!, "UpdateUser", data, context, global::SnapTrace.SnapStatus.Call);
            @this.UpdateUser(name);
            CallRecord_SnapTrace(null!, "UpdateUser", null, context, global::SnapTrace.SnapStatus.Return);
        }
    }
}
