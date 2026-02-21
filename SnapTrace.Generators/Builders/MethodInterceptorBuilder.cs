using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnapTrace.Generators.Definitions;

namespace SnapTrace.Generators.Builders;

public class MethodInterceptorBuilder
{
    // Builder data
    private ReturnDefinition _return = new("void", true, false, false);
    private readonly List<ParameterDefinition> _params = new();
    private readonly List<string> _locations = new();

    // Method Metadata
    private readonly string _methodName;
    private readonly string _className;
    private string? _typeParameters;
    private string? _whereConstraints;
    private MethodSituation _situation;

    public MethodInterceptorBuilder(string className, string methodName, MethodSituation situation)
    {
        _className = className;
        _methodName = methodName;
        _situation = situation;
    }

    // --- Standard additions ---

    public MethodInterceptorBuilder WithReturn(string type, bool deepCopy = false, bool redacted = false)
    {
        bool isVoid = type == "void";
        _return = new ReturnDefinition(type, isVoid, deepCopy, redacted);
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
        _params.Add(new ParameterDefinition(name, type, modifier, isParams, deepCopy, redacted));
        return this;
    }

    public MethodInterceptorBuilder AddLocation(string path, int line, int col)
    {
        var safePath = path.Replace("\\", "\\\\");
        _locations.Add($@"[global::System.Runtime.CompilerServices.InterceptsLocation(@""{safePath}"", {line}, {col})]");
        return this;
    }

    // --- The Build Engine ---

    internal void InternalBuild(StringBuilder sb)
    {
        // TODO
    }
}
