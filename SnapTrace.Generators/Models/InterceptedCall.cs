namespace SnapTrace.Generators.Models;

internal record InterceptedCall(
    string InterceptorAttributeString,
    MethodModel Method,
    ClassModel Class);
