using System;
using System.Collections.Generic;
using System.Linq;

namespace SnapTrace.Generators.Builders;

public class MethodInterceptorBuilder
{
    private readonly List<string> _locations = new();
    private readonly List<(string Name, string Type, bool Redacted, bool DeepCopy)> _params = new();

    // Method Metadata
    private string _methodName = "";
    private string _className = "";
    private string _returnType = "void";
    private bool _isVoid = true;
    private bool _isStatic = false;
    private bool _isAsync = false;

    // Policy Overrides
    private bool _methodDeepCopy = false;
    private bool _methodRedacted = false;
    private bool _returnDeepCopy = false;
    private bool _returnRedacted = false;
    private readonly List<string> _contextMembers = new();

    public MethodInterceptorBuilder(string className, string methodName, bool isStatic, bool isAsync)
    {
        _className = className;
        _methodName = methodName;
        _isStatic = isStatic;
    }

    public MethodInterceptorBuilder WithReturn(string type, bool isVoid = false)
    {
        _returnType = type;
        _isVoid = isVoid;
        return this;
    }

    public MethodInterceptorBuilder AddParameter(string name, string type)
    {
        _params.Add((name, type, false, false));
        return this;
    }

    // --- Policy Refinements ---

    public MethodInterceptorBuilder RedactParameter(string name)
    {
        var idx = _params.FindIndex(p => p.Name == name);
        if (idx != -1) _params[idx] = _params[idx] with { Redacted = true };
        return this;
    }

    public MethodInterceptorBuilder DeepCopyParameter(string name)
    {
        var idx = _params.FindIndex(p => p.Name == name);
        if (idx != -1) _params[idx] = _params[idx] with { DeepCopy = true };
        return this;
    }

    public MethodInterceptorBuilder WithMethodPolicies(bool deepCopy, bool redacted)
    {
        _methodDeepCopy = deepCopy;
        _methodRedacted = redacted;
        return this;
    }

    public MethodInterceptorBuilder WithReturnPolicies(bool deepCopy, bool redacted)
    {
        _returnDeepCopy = deepCopy;
        _returnRedacted = redacted;
        return this;
    }

    public MethodInterceptorBuilder CaptureContext(string memberName)
    {
        _contextMembers.Add(memberName);
        return this;
    }

    public MethodInterceptorBuilder AddLocation(string path, int line, int col)
    {
        _locations.Add($@"[global::System.Runtime.CompilerServices.InterceptsLocation(@""{path}"", {line}, {col})]");
        return this;
    }

    // --- The Build Engine ---

    internal string InternalBuild()
    {
        var signatureParams = (_isStatic ? "" : $"this {_className} instance") +
            (_params.Any() && !_isStatic ? ", " : "") +
            string.Join(", ", _params.Select(p => $"{p.Type} {p.Name}"));

        var callDataElements = _params.Select(p =>
            p.Redacted ? "\"REDACTED\"" :
            (p.DeepCopy || _methodDeepCopy) ? $"(object?){p.Name}.Clone()" : $"(object?){p.Name}");

        var callDataArray = _params.Any() ? $"new object?[] {{ {string.Join(", ", callDataElements)} }}" : "global::System.Array.Empty<object?>()";
        var contextObj = _contextMembers.Any() ? $"new {{ {string.Join(", ", _contextMembers.Select(m => $"{m} = instance.{m}"))} }}" : "null";
        var callTarget = _isStatic ? _className : "instance";
        var passThrough = string.Join(", ", _params.Select(p => p.Name));

        string execution;
        if (_isVoid)
        {
            execution = $@"{callTarget}.{_methodName}({passThrough});
            global::SnapTrace.SnapTraceObserver.Record(new global::SnapTrace.Models.SnapEntry(""{_className}.{_methodName}"", null, {contextObj}, global::SnapTrace.Models.SnapStatus.Return));";
        }
        else
        {
            var logExpr = (_returnRedacted || _methodRedacted) ? "\"REDACTED\"" :
                         (_returnDeepCopy || _methodDeepCopy) ? "result.Clone()" : "result";
            execution = $@"var result = {callTarget}.{_methodName}({passThrough});
            global::SnapTrace.SnapTraceObserver.Record(new global::SnapTrace.Models.SnapEntry(""{_className}.{_methodName}"", new object?[] {{ (object?){logExpr} }}, {contextObj}, global::SnapTrace.Models.SnapStatus.Return));
            return result;";
        }

        return $@"
    {string.Join("\n    ", _locations)}
    public static {_returnType} Intercepted_{_className}_{_methodName}({signatureParams})
    {{
        global::SnapTrace.SnapTraceObserver.Record(new global::SnapTrace.Models.SnapEntry(""{_className}.{_methodName}"", {callDataArray}, {contextObj}, global::SnapTrace.Models.SnapStatus.Call));
        {execution}
    }}";
    }
}
