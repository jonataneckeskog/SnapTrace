using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnapTrace.Generators.Definitions;

namespace SnapTrace.Generators.Builders;

public class ClassInterceptorBuilder
{
    private readonly string _className;
    private readonly ClassSituation _situation;
    private string? _typeParameters;
    private string? _whereConstraints;

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

    internal void InternalBuild(StringBuilder sb)
    {
        sb.AppendLine($"internal static class {_className}SnapInterceptor");
        sb.AppendLine("{");

        // 1. Generate the UnsafeAccessor inside the static class
        // This allows every intercepted method in this class to use CallRecord
        sb.AppendLine("    [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.StaticMethod, Name = \"Record\")]");
        sb.AppendLine("    extern static void CallRecord_SnapTrace(string method, object? data, object? context, global::SnapTrace.SnapStatus status)");
        sb.AppendLine();

        // 2. Generate the GetContext accessor, which methods can call to get the extra context
        sb.AppendLine("    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("    private static string GetClassContext_SnapTrace()");
        sb.AppendLine("    {");

        sb.Append("        return ");
        if (_contextMembers.Count == 0)
        {
            sb.AppendLine("string.Empty;");
        }
        else
        {
            sb.Append(string.Join(", ", _contextMembers));
            sb.AppendLine(";");
        }

        sb.AppendLine("    }");
        sb.AppendLine();

        // 2. Loop through and build the individual method interceptors
        foreach (var method in _methods)
        {
            method.InternalBuild(sb);
        }

        sb.AppendLine("}");
    }
}
