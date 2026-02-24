[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
public static void MyTestMethod_SnapTrace_void()
{
    object[] data = null;

    var contextBefore = null;
    CallRecord_SnapTrace(null!, "MyTestMethod", data, contextBefore, global::SnapTrace.Runtime.Models.SnapStatus.Call);

    global::MyNamespace.MyTestClass.MyTestMethod();

    var contextAfter = null;
    CallRecord_SnapTrace(null!, "MyTestMethod", null, contextAfter, global::SnapTrace.Runtime.Models.SnapStatus.Return);
}
