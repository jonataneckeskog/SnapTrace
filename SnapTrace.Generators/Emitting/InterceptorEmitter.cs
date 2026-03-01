using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Text;
using SnapTrace.Generators.Models;

namespace SnapTrace.Generators.Emitting;

internal static class InterceptorEmitter
{
    private const string SnapStatusPath = "global::SnapTrace.Runtime.Models.SnapStatus";

    public static string Emit(ClassModel model)
    {
        var sb = new StringBuilder();

        using (var sw = new StringWriter(sb))
        using (var w = new IndentedTextWriter(sw, "    "))
        {
            EmitFileHeader(w);
            w.WriteLine();
            EmitNamespaceOpen(w, model);
            w.WriteLine();
            EmitClassBody(w, model);
            EmitNamespaceClose(w, model);
        }

        return sb.ToString();
    }

    private static void EmitFileHeader(IndentedTextWriter w)
    {
        w.WriteLine("#nullable enable");
        w.WriteLine("using global::SnapTrace;");
    }

    private static void EmitNamespaceOpen(IndentedTextWriter w, ClassModel model)
    {
        string ns = string.IsNullOrWhiteSpace(model.Namespace)
            ? "SnapTrace.Generated"
            : $"SnapTrace.Generated.{model.Namespace}";

        w.WriteLine($"namespace {ns}");
        w.Write("{");
        w.Indent++;
    }

    private static void EmitNamespaceClose(IndentedTextWriter w, ClassModel model)
    {
        w.Indent--;
        w.WriteLine("}");
    }

    private static void EmitClassBody(IndentedTextWriter w, ClassModel model)
    {
        bool isStatic = model.Situation.HasFlag(ClassSituation.Static);
        bool isStruct = model.Situation.HasFlag(ClassSituation.IsStruct) || model.Situation.HasFlag(ClassSituation.IsRefStruct);
        bool isGeneric = !string.IsNullOrWhiteSpace(model.TypeParameters);

        // Build target type
        string targetType = string.IsNullOrWhiteSpace(model.Namespace)
            ? $"global::{model.Name}{model.TypeParameters}"
            : $"global::{model.Namespace}.{model.Name}{model.TypeParameters}";

        // Class declaration
        string classDecl = $"internal static class {model.Name}_SnapTrace{model.TypeParameters}";
        w.WriteLine(classDecl);

        if (isGeneric && !string.IsNullOrWhiteSpace(model.WhereConstraints))
        {
            w.Indent++;
            w.WriteLine(model.WhereConstraints);
            w.Indent--;
        }

        w.WriteLine("{");
        w.Indent++;

        // UnsafeAccessor for Record
        w.WriteLine("""[global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.StaticMethod, Name = "Record")]""");
        w.WriteLine($"""extern static void CallRecord_SnapTrace(global::SnapTrace.SnapTraceObserver? target, string method, object? data, object? context, {SnapStatusPath} status);""");
        w.InnerWriter.WriteLine();

        // Parameter logic
        string thisParam = isStatic ? "" : (isStruct ? $"ref {targetType} @this" : $"{targetType} @this");
        string thisArg = isStatic ? "" : (isStruct ? "ref @this" : "@this");

        // Context member accessors
        foreach (var member in model.ContextMembers)
        {
            string accessorKind = isStatic ? "StaticField" : "Field";
            w.WriteLine($"[global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.{accessorKind}, Name = \"{member.Name}\")]");
            w.WriteLine($"extern static ref {member.Type} Get_{member.Name}_SnapTrace({thisParam});");
            w.InnerWriter.WriteLine();
        }

        // GetClassContext_SnapTrace
        w.WriteLine("[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        w.WriteLine($"private static object? GetClassContext_SnapTrace({thisParam})");
        w.WriteLine("{");
        w.Indent++;

        if (model.ContextMembers.Count == 0)
        {
            w.WriteLine("return null;");
        }
        else
        {
            var joinedArgs = string.Join(", ", model.ContextMembers.Select(m => $"{m.Name} = (object?)Get_{m.Name}_SnapTrace({thisArg})"));
            w.WriteLine($"return new {{ {joinedArgs} }};");
        }

        w.Indent--;
        w.WriteLine("}");

        // Methods
        foreach (var method in model.Methods)
        {
            w.InnerWriter.WriteLine();
            EmitMethod(w, model, method, targetType);
        }

        w.Indent--;
        w.WriteLine("}");
    }

    private static void EmitMethod(IndentedTextWriter w, ClassModel classModel, MethodModel method, string targetType)
    {
        bool isMethodStatic = method.Situation.HasFlag(MethodSituation.Static);
        bool isStaticClass = classModel.Situation.HasFlag(ClassSituation.Static);
        bool isStruct = classModel.Situation.HasFlag(ClassSituation.IsStruct) || classModel.Situation.HasFlag(ClassSituation.IsRefStruct);
        string refModifier = isStruct ? "ref " : "";

        string saveContext;
        if (isStaticClass)
        {
            saveContext = "GetClassContext_SnapTrace()";
        }
        else if (isMethodStatic)
        {
            saveContext = "null";
        }
        else
        {
            saveContext = $"GetClassContext_SnapTrace({refModifier}@this)";
        }

        // InterceptsLocation attributes
        foreach (var loc in method.InterceptLocations)
        {
            w.WriteLine(loc);
        }

        // MethodImpl
        w.WriteLine("[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");

        // Interceptor method name
        var safeReturnType = GetSafeTypeName(method.ReturnType);
        var interceptorName = $"{method.Name}_SnapTrace_{safeReturnType}";

        if (method.Parameters.Count > 0)
        {
            var paramPart = string.Join("_", method.Parameters.Select(p => GetSafeTypeName(p.Type)));
            interceptorName += $"_{paramPart}";
        }

        // Modifiers
        var modifiers = "public static";
        if (method.Situation.HasFlag(MethodSituation.Async)) modifiers += " async";
        if (method.Situation.HasFlag(MethodSituation.Unsafe)) modifiers += " unsafe";

        var returnStr = method.ReturnType;
        bool isVoid = returnStr == "void" || returnStr == "System.Void" || returnStr == "global::System.Void";

        if (!isVoid)
        {
            if (method.Situation.HasFlag(MethodSituation.ReturnsRef))
                returnStr = "ref " + returnStr;
            else if (method.Situation.HasFlag(MethodSituation.ReturnsRefReadonly))
                returnStr = "ref readonly " + returnStr;
        }

        // Method parameters
        var methodParams = new System.Collections.Generic.List<string>();
        if (!isMethodStatic)
        {
            string thisModifier = isStruct ? "ref " : "";
            methodParams.Add($"this {thisModifier}{targetType} @this");
        }

        foreach (var p in method.Parameters)
        {
            var prefix = string.IsNullOrEmpty(p.Modifier) ? "" : $"{p.Modifier} ";
            if (p.IsParams) prefix += "params ";
            methodParams.Add($"{prefix}{p.Type} {p.Name}");
        }

        // Write method signature
        bool hasGenerics = !string.IsNullOrEmpty(method.TypeParameters);

        w.Write($"{modifiers} {returnStr} {interceptorName}");
        if (hasGenerics) w.Write($"{method.TypeParameters}");
        w.Write($"({string.Join(", ", methodParams)})");

        if (hasGenerics && !string.IsNullOrEmpty(method.WhereConstraints))
        {
            w.Write($" {method.WhereConstraints}");
        }

        w.WriteLine();
        w.WriteLine("{");
        w.Indent++;

        // Parameter data array
        w.Write("object[]? data = ");
        if (method.Parameters.Count == 0)
        {
            w.WriteLine("null;");
        }
        else
        {
            w.Write("new object[] { ");
            var arrayParts = method.Parameters.Select(p =>
            {
                if (p.Redacted)
                    return $"/* {p.Name} */ \"[REDACTED]\"";

                if (p.DeepCopy)
                    return $"/* {p.Name} */ global::SnapTrace.Generated.SnapCloner.Clone((object){p.Name})";

                return $"/* {p.Name} */ ((object){p.Name})?.ToString() ?? \"null\"";
            });

            w.Write(string.Join(", ", arrayParts));
            w.WriteLine(" };");
        }
        w.InnerWriter.WriteLine();

        // Context before
        w.WriteLine($"var contextBefore = {saveContext};");

        // Record entry
        w.WriteLine($"CallRecord_SnapTrace(null!, \"{method.Name}\", data, contextBefore, {SnapStatusPath}.Call);");
        w.InnerWriter.WriteLine();

        // Execute and capture return
        var target = isMethodStatic ? targetType : "@this";
        string callArgs = string.Join(", ", method.Parameters.Select(p => p.IsNonNullable ? $"{p.Name}!" : p.Name));

        if (method.IsVoid)
        {
            w.WriteLine($"{target}.{method.Name}({callArgs});");
            w.InnerWriter.WriteLine();
            w.WriteLine($"var contextAfter = {saveContext};");
            w.WriteLine($"CallRecord_SnapTrace(null!, \"{method.Name}\", null, contextAfter, {SnapStatusPath}.Return);");
        }
        else
        {
            string resultRefModifier;
            string refCallModifier;

            if (method.Situation.HasFlag(MethodSituation.ReturnsRef))
            {
                resultRefModifier = "ref var ";
                refCallModifier = "ref ";
            }
            else if (method.Situation.HasFlag(MethodSituation.ReturnsRefReadonly))
            {
                resultRefModifier = "ref readonly var ";
                refCallModifier = "ref ";
            }
            else
            {
                resultRefModifier = "var ";
                refCallModifier = "";
            }

            w.WriteLine($"{resultRefModifier}result = {refCallModifier}{target}.{method.Name}({callArgs});");
            w.InnerWriter.WriteLine();

            string recordedResult;
            if (method.RedactedReturn)
            {
                recordedResult = "\"[REDACTED]\"";
            }
            else if (method.DeepCopyReturn)
            {
                recordedResult = $"global::SnapTrace.Generated.SnapCloner.Clone((object)result)";
            }
            else
            {
                recordedResult = "((object)result)?.ToString() ?? \"null\"";
            }

            w.WriteLine($"var contextAfter = {saveContext};");
            w.WriteLine($"CallRecord_SnapTrace(null!, \"{method.Name}\", {recordedResult}, contextAfter, {SnapStatusPath}.Return);");
            w.InnerWriter.WriteLine();

            string returnModifier = (method.Situation.HasFlag(MethodSituation.ReturnsRef) || method.Situation.HasFlag(MethodSituation.ReturnsRefReadonly))
                ? "ref "
                : "";

            w.WriteLine($"return {returnModifier}result;");
        }

        // Close method
        w.Indent--;
        w.WriteLine("}");
    }

    private static string GetSafeTypeName(string type)
    {
        return type
            .Replace("global::", "")
            .Replace("[]", "Array")
            .Replace("<", "_")
            .Replace(">", "_")
            .Replace("(", "Tup_")
            .Replace(")", "_")
            .Replace(",", "_")
            .Replace(" ", "")
            .Replace(".", "_")
            .TrimEnd('_');
    }
}
