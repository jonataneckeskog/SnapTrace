using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
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
    private string _typeParameters = "";
    private string? _whereConstraints;
    private MethodSituation _situation;
    private ClassSituation _classSituation;

    public MethodInterceptorBuilder(string methodName, MethodSituation situation, ClassSituation classSituation)
    {
        _methodName = methodName;
        _situation = situation;
        _classSituation = classSituation;
    }

    // --- Standard additions ---

    public MethodInterceptorBuilder WithReturn(string type, bool deepCopy = false, bool redacted = false)
    {
        bool isVoid = type == "void";
        _return = new ReturnDefinition(type, isVoid, deepCopy, redacted);
        return this;
    }

    public MethodInterceptorBuilder WithGenerics(string typeParameters, string? whereConstraints = null)
    {
        if (string.IsNullOrWhiteSpace(typeParameters))
            throw new ArgumentException("Type parameters cannot be empty.");

        if (typeParameters[0] != '<' || typeParameters[typeParameters.Length - 1] != '>')
            throw new ArgumentException("Type parameters must be enclosed in angle brackets, e.g., '<T>'.");

        _typeParameters = typeParameters;
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

    internal void InternalBuild(IndentedTextWriter writer, string targetType)
    {
        // 1. Evaluate Class and Method Situations
        bool isMethodStatic = _situation.HasFlag(MethodSituation.Static);
        bool isStaticClass = _classSituation.HasFlag(ClassSituation.Static);
        bool isStruct = _classSituation.HasFlag(ClassSituation.IsStruct) || _classSituation.HasFlag(ClassSituation.IsRefStruct);

        // 2. Append InterceptsLocation
        foreach (var loc in _locations)
        {
            writer.WriteLine(loc);
        }

        // 3. Append method options
        writer.WriteLine("[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");

        // 4. Construct the strict interceptor method name
        var safeReturnType = GetSafeTypeName(_return.Type);
        var interceptorName = $"{_methodName}_SnapTrace_{safeReturnType}";

        if (_params.Count > 0)
        {
            var paramPart = string.Join("_", _params.Select(p => GetSafeTypeName(p.Type)));
            interceptorName += $"_{paramPart}";
        }

        // 5. Resolve MethodSituation (Modifiers & Ref Returns)
        var modifiers = "public static";
        if (_situation.HasFlag(MethodSituation.Async)) modifiers += " async";
        if (_situation.HasFlag(MethodSituation.Unsafe)) modifiers += " unsafe";

        var returnStr = _return.Type;
        if (_situation.HasFlag(MethodSituation.ReturnsRef)) returnStr = "ref " + returnStr;
        else if (_situation.HasFlag(MethodSituation.ReturnsRefReadonly)) returnStr = "ref readonly " + returnStr;

        // 6. Construct Method Parameters
        var methodParams = new List<string>();
        if (!isMethodStatic)
        {
            string thisModifier = isStruct ? "ref " : "";
            methodParams.Add($"{thisModifier}{targetType} @this");
        }

        foreach (var p in _params)
        {
            var prefix = string.IsNullOrEmpty(p.Modifier) ? "" : $"{p.Modifier} ";
            if (p.IsParams) prefix += "params ";
            methodParams.Add($"{prefix}{p.Type} {p.Name}");
        }

        // 7. Write Method Signature
        bool IsGeneric = !string.IsNullOrEmpty(_typeParameters);

        writer.Write($"{modifiers} {returnStr} {interceptorName}");
        if (IsGeneric) writer.Write($"{_typeParameters}");
        writer.Write($"({string.Join(", ", methodParams)})");

        if (IsGeneric && !string.IsNullOrEmpty(_whereConstraints))
        {
            writer.Write($" {_whereConstraints}");
        }

        writer.WriteLine();
        writer.WriteLine("{");
        writer.Indent++;

        // 8. Save method parameters to a tuple
        writer.Write("object? data = ");
        if (_params.Count == 0)
        {
            writer.WriteLine("null;");
        }
        else
        {
            writer.Write("(");
            var tupleParts = _params.Select(p =>
            {
                if (p.Redacted) return $"{p.Name}: \"[REDACTED]\"";
                if (p.DeepCopy) return $"{p.Name}: {p.Name} is null ? default : global::System.Text.Json.JsonSerializer.Deserialize<{p.Type}>(global::System.Text.Json.JsonSerializer.SerializeToUtf8Bytes({p.Name}))";
                return $"{p.Name}: {p.Name}";
            });
            writer.Write(string.Join(", ", tupleParts));
            writer.WriteLine(");");
        }

        // 9. Save the context
        if (isStaticClass)
        {
            writer.WriteLine("var context = GetClassContext_SnapTrace();");
        }
        else if (isMethodStatic)
        {
            writer.WriteLine("object? context = null;");
        }
        else
        {
            string refModifier = isStruct ? "ref " : "";
            writer.WriteLine($"var context = GetClassContext_SnapTrace({refModifier}@this);");
        }
        writer.InnerWriter.WriteLine();

        // 10. Record the Entry
        writer.WriteLine($"CallRecord_SnapTrace(null!, \"{_methodName}\", data, context, global::SnapTrace.SnapStatus.Call);");

        // 11. EXECUTE ORIGINAL AND CAPTURE RETURN
        var target = isMethodStatic ? targetType : "@this";
        string callArgs = string.Join(", ", _params.Select(p => p.Name));

        if (_return.IsVoid)
        {
            writer.WriteLine($"{target}.{_methodName}({callArgs});");
            writer.WriteLine($"CallRecord_SnapTrace(null!, \"{_methodName}\", null, context, global::SnapTrace.SnapStatus.Return);");
        }
        else
        {
            writer.WriteLine($"var result = {target}.{_methodName}({callArgs});");
            writer.WriteLine($"CallRecord_SnapTrace(null!, \"{_methodName}\", result, context, global::SnapTrace.SnapStatus.Return);");
            writer.InnerWriter.WriteLine();
            writer.WriteLine("return result;");
        }

        // 12. Close the method
        writer.Indent--;
        writer.WriteLine("}");
    }

    private static string GetSafeTypeName(string type)
    {
        return type
            .Replace("global::", "")
            .Replace("[]", "Array")
            .Replace("<", "_")
            .Replace(">", "_")
            .Replace(",", "_")
            .Replace(" ", "")
            .Replace(".", "_")
            .TrimEnd('_');
    }
}
