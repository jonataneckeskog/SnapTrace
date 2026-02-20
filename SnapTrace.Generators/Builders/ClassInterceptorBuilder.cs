using System;
using System.Collections.Generic;
using System.Linq;

namespace SnapTrace.Generators.Builders;

public class ClassInterceptorBuilder
{
    private readonly string _className;
    private readonly ClassSituation _situation;
    private string _typeParameters = string.Empty;
    private string _whereConstraints = string.Empty;

    private readonly HashSet<string> _contextMembers = new();
    private readonly List<MethodInterceptorBuilder> _methods = new();

    public ClassInterceptorBuilder(string className, ClassSituation situation)
    {
        _className = className;
        _situation = situation;
    }

    public ClassInterceptorBuilder WithTypeParameters(string typeParameters)
    {
        _typeParameters = typeParameters;
        return this;
    }

    public ClassInterceptorBuilder WithWhereConstraints(string whereConstraints)
    {
        _whereConstraints = whereConstraints;
        return this;
    }

    public ClassInterceptorBuilder WithMethod(string name, MethodSituation situation, Action<MethodInterceptorBuilder> config)
    {
        var mb = new MethodInterceptorBuilder(_className, name, situation);

        config(mb);
        _methods.Add(mb);
        return this;
    }

    public ClassInterceptorBuilder AddContextMember(string memberName)
    {
        _contextMembers.Add(memberName);
        return this;
    }

    internal string InternalBuild()
    {
        var (helperMethod, methodsSource) = BuildParts();

        var unsafeKeyword = _situation.HasFlag(ClassSituation.Unsafe) ? "unsafe " : "";

        return $@"
public {unsafeKeyword}static class {_className}_Interceptors{_typeParameters}
{_whereConstraints}
{{
{helperMethod}

{methodsSource}
}}";
    }

    private (string, string) BuildParts()
    {
        string helperMethod;
        if (_situation.HasFlag(ClassSituation.Static))
        {
            var contextBody = _contextMembers.Any()
                ? $"new {{ {string.Join(", ", _contextMembers.Select(m => $"{_className}.{m}"))} }}"
                : "null";

            helperMethod = $"    private static object? GetContext() => {contextBody};";
        }
        else
        {
            var contextBody = _contextMembers.Any()
                ? $"new {{ {string.Join(", ", _contextMembers.Select(m => $"instance.{m}"))} }}"
                : "null";

            var instanceType = _className;
            if (_situation.HasFlag(ClassSituation.IsStruct) || _situation.HasFlag(ClassSituation.IsRefStruct))
            {
                // To avoid copying structs, we can pass them by readonly reference.
                instanceType = $"in {instanceType}";
            }

            helperMethod = $"    private static object? GetContext({instanceType} instance) => {contextBody};";
        }

        var methodsSource = string.Join("\n", _methods.Select(m => m.InternalBuild()));

        return (helperMethod, methodsSource);
    }
}
