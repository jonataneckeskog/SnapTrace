using System;
using System.Collections.Generic;
using System.Linq;

namespace SnapTrace.Generators.Builders;

public class MethodInterceptorBuilder
{
    // Builder data
    private (string Type, bool IsVoid, bool DeepCopy, bool Redacted) _return = ("void", true, false, false);
    private readonly List<(string Name, string Type, string Modifier, bool IsParams, bool DeepCopy, bool Redacted)> _params = new();
    private readonly List<string> _locations = new();

    // Method Metadata
    private readonly string _methodName;
    private readonly string _className;
    private string _typeParameters;
    private string _whereConstraints;
    private MethodSituation _situation;

    public MethodInterceptorBuilder(string className, string methodName, MethodSituation situation)
    {
        _className = className;
        _methodName = methodName;
        _situation = situation;
    }

    // --- Standard additions ---

    public MethodInterceptorBuilder WithReturn(string type = "void", bool isVoid = true, bool deepCopy = false, bool redacted = false)
    {
        if (isVoid) return this;

        _return = (type, isVoid, deepCopy, redacted);
        return this;
    }

    public MethodInterceptorBuilder WithTypeParameters(string typeParameters)
    {
        _typeParameters = typeParameters;
        return this;
    }

    public MethodInterceptorBuilder WithWhereConstraints(string whereConstraints)
    {
        _whereConstraints = whereConstraints;
        return this;
    }

    public MethodInterceptorBuilder WithParameter(string name, string type, string modifier = "", bool isParams = false, bool deepCopy = false, bool redacted = false)
    {
        _params.Add((name, type, modifier, isParams, deepCopy, redacted));
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
        return $@"TODO";
    }
}
