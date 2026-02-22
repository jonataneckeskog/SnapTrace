[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
public static async void MyTestMethod_SnapTrace_void(global::MyNamespace.MyTestClass @this)
{
    object? data = null;
    var context = GetClassContext_SnapTrace(@this);

    CallRecord_SnapTrace(null!, "MyTestMethod", data, context, global::SnapTrace.SnapStatus.Call);
    @this.MyTestMethod();
    CallRecord_SnapTrace(null!, "MyTestMethod", null, context, global::SnapTrace.SnapStatus.Return);
}
