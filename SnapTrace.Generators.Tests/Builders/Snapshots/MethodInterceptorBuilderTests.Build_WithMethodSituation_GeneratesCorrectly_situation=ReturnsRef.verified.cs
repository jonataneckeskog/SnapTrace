[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
public static void MyTestMethod_SnapTrace_void(global::MyNamespace.MyTestClass @this)
{
    object? data = null;
    var context = GetClassContext_SnapTrace(@this);

    CallRecord_SnapTrace(null!, "MyTestMethod", data, context, global::SnapTrace.Runtime.Models.SnapStatus.Call);
    @this.MyTestMethod();
    CallRecord_SnapTrace(null!, "MyTestMethod", null, context, global::SnapTrace.Runtime.Models.SnapStatus.Return);
}
