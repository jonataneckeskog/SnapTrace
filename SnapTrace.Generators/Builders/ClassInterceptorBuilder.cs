using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using SnapTrace.Generators.Definitions;

namespace SnapTrace.Generators.Builders;

public class ClassInterceptorBuilder
{
    private readonly string _fullyQualifiedName;
    private readonly string _className;
    private readonly ClassSituation _situation;
    private string? _typeParameters;
    private string? _whereConstraints;

    private readonly HashSet<(string Name, string Type)> _contextMembers = new();
    private readonly List<MethodInterceptorBuilder> _methods = new();

    public ClassInterceptorBuilder(string fullyQualifiedName, string className, ClassSituation situation)
    {
        _fullyQualifiedName = fullyQualifiedName;
        _className = className;
        _situation = situation;
    }

    public ClassInterceptorBuilder WithMethod(string methodName, MethodSituation situation, Action<MethodInterceptorBuilder> config)
    {
        // Create the full class name, with generics
        string fullName = $"{_fullyQualifiedName}.{_className}";
        if (_situation.HasFlag(ClassSituation.IsGeneric))
        {
            fullName = $"{_fullyQualifiedName}.{_className}<{_typeParameters}>";
        }

        var mb = new MethodInterceptorBuilder(fullName, methodName, situation, _situation);
        config(mb);
        _methods.Add(mb);
        return this;
    }

    public ClassInterceptorBuilder AddContextMember(string name, string type)
    {
        _contextMembers.Add((name, type));
        return this;
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

    internal void InternalBuild(IndentedTextWriter writer)
    {
        // 1. Evaluate situations
        bool isStatic = _situation.HasFlag(ClassSituation.Static);
        bool isStruct = _situation.HasFlag(ClassSituation.IsStruct) || _situation.HasFlag(ClassSituation.IsRefStruct);
        bool isGeneric = _situation.HasFlag(ClassSituation.IsGeneric);

        // 2. Build the target type safely
        string targetType = $"{_fullyQualifiedName}.{_className}";

        if (isGeneric && !string.IsNullOrWhiteSpace(_typeParameters))
        {
            string generics = _typeParameters!.Trim().StartsWith("<") ? _typeParameters : $"<{_typeParameters}>";
            targetType += generics;
        }

        // 3. Class declaration
        string classDecl = $"internal static class {_className}_SnapTrace";
        if (isGeneric && !string.IsNullOrWhiteSpace(_typeParameters))
        {
            classDecl += _typeParameters!.Trim().StartsWith("<") ? _typeParameters : $"<{_typeParameters}>";
        }

        writer.WriteLine(classDecl);

        if (isGeneric && !string.IsNullOrWhiteSpace(_whereConstraints))
        {
            writer.Indent++;
            writer.WriteLine(_whereConstraints);
            writer.Indent--;
        }

        writer.WriteLine("{");
        writer.Indent++;

        // 4. Static Record Accessor
        writer.WriteLine("[global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.StaticMethod, Name = \"Record\")]");
        writer.WriteLine("extern static void CallRecord_SnapTrace(global::SnapTrace.SnapTracer? target, string method, object? data, object? context, global::SnapTrace.SnapStatus status);");
        writer.WriteLine();

        // Parameter logic
        string thisParam = isStatic ? "" : (isStruct ? $"ref {targetType} @this" : $"{targetType} @this");
        string thisArg = isStatic ? "" : (isStruct ? "ref @this" : "@this");

        // 5. Generate Accessors
        foreach (var member in _contextMembers)
        {
            string accessorKind = isStatic ? "StaticField" : "Field";
            writer.WriteLine($"[global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.{accessorKind}, Name = \"{member.Name}\")]");
            writer.WriteLine($"extern static ref {member.Type} Get_{member.Name}_SnapTrace({thisParam});");
            writer.WriteLine();
        }

        // 6. The Context Method
        writer.WriteLine("[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        writer.WriteLine($"private static object? GetClassContext_SnapTrace({thisParam})");
        writer.WriteLine("{");
        writer.Indent++;

        if (_contextMembers.Count == 0)
        {
            writer.WriteLine("return null;");
        }
        else
        {
            var joinedArgs = string.Join(", ", _contextMembers.Select(m => $"{m.Name} = (object?)Get_{m.Name}_SnapTrace({thisArg})"));
            writer.WriteLine($"return new {{ {joinedArgs} }};");
        }

        writer.Indent--;
        writer.WriteLine("}");

        // 7. Pass the writer along to method builders
        foreach (var __method in _methods)
        {
            writer.WriteLine();
            __method.InternalBuild(writer);
        }

        writer.Indent--;
        writer.WriteLine("}");
    }
}
