#nullable enable
using global::SnapTrace;

namespace SnapTrace.Generated.MyNamespace
{
    internal static class MyClass_SnapTrace<T>
        where T : class
    {
        [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.StaticMethod, Name = "Record")]
        extern static void CallRecord_SnapTrace(global::SnapTrace.SnapTraceObserver? target, string method, object? data, object? context, global::SnapTrace.Runtime.Models.SnapStatus status);

        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static object? GetClassContext_SnapTrace(global::MyNamespace.MyClass<T> @this)
        {
            return null;
        }
    }
}
