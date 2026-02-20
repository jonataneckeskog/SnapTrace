using System;
using System.Collections.Generic;
using System.Linq;

namespace SnapTrace.Generators.Builders;

public class ClassInterceptorBuilder
{
    private readonly string _className;
    private readonly bool _isStatic;

    private readonly HashSet<string> _contextMembers = new();
    private readonly List<MethodInterceptorBuilder> _methods = new();

    public ClassInterceptorBuilder(string className, bool isStatic)
    {
        _className = className;
        _isStatic = isStatic;
    }

    public ClassInterceptorBuilder WithMethod(string name, Action<MethodInterceptorBuilder> config, bool isStatic, bool isAsync)
    {
        var mb = new MethodInterceptorBuilder(_className, name, isStatic, isAsync);
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
        // Build the private helper method that resolves the hook
        var contextBody = _contextMembers.Any()
            ? $"new {{ {string.Join(", ", _contextMembers.Select(m => $"instance.{m}"))} }}"
            : "null";

        var helperMethod = $@"
    private static object? GetContext({_className} instance) => {contextBody};";

        var methodsSource = string.Join("\n", _methods.Select(m => m.InternalBuild()));

        return $@"
public static class {_className}_Interceptors
{{
    {helperMethod}
    {methodsSource}
}}";
    }
}
