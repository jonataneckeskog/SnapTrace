[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
public static void MyTestMethod_SnapTrace_void(this ref global::MyNamespace.MyTestClass @this)
{
    object[]? data = null;

    var contextBefore = GetClassContext_SnapTrace(ref @this);
    CallRecord_SnapTrace(null!, "MyTestMethod", data, contextBefore, global::SnapTrace.Runtime.Models.SnapStatus.Call);

    @this.MyTestMethod();

    var contextAfter = GetClassContext_SnapTrace(ref @this);
    CallRecord_SnapTrace(null!, "MyTestMethod", null, contextAfter, global::SnapTrace.Runtime.Models.SnapStatus.Return);
}
