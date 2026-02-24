namespace SnapTrace.Generators.Models;

/// <summary>
/// Model 'collected' by the generator and later send to Builders
/// to handle building the syntax.
/// </summary>
/// <param name="InterceptorAttributeString"></param>
/// <param name="Method"></param>
/// <param name="Class"></param>
internal record InterceptedCall(
    string InterceptorAttributeString,
    MethodData Method,
    ClassData Class);
