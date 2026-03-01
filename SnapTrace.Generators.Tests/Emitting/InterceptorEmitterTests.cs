using System;
using SnapTrace.Generators.Emitting;
using SnapTrace.Generators.Models;

namespace SnapTrace.Generators.Tests.Emitting;

public class InterceptorEmitterTests
{
    private static ClassModel MakeClass(
        string ns = "MyNamespace",
        string name = "MyClass",
        ClassSituation situation = ClassSituation.None,
        string typeParameters = "",
        string whereConstraints = "",
        ContextMemberModel[]? contextMembers = null,
        MethodModel[]? methods = null)
    {
        return new ClassModel(
            ns, name,
            string.IsNullOrWhiteSpace(ns) ? $"global::{name}" : $"global::{ns}.{name}",
            situation,
            typeParameters,
            whereConstraints,
            contextMembers ?? Array.Empty<ContextMemberModel>(),
            methods ?? Array.Empty<MethodModel>());
    }

    private static MethodModel MakeMethod(
        string name = "MyTestMethod",
        string returnType = "void",
        bool isVoid = true,
        MethodSituation situation = MethodSituation.None,
        string typeParameters = "",
        string whereConstraints = "",
        ParameterModel[]? parameters = null,
        bool deepCopyReturn = false,
        bool redactedReturn = false,
        string[]? interceptLocations = null)
    {
        return new MethodModel(
            name, returnType, isVoid, situation,
            typeParameters, whereConstraints,
            parameters ?? Array.Empty<ParameterModel>(),
            deepCopyReturn, redactedReturn,
            interceptLocations ?? Array.Empty<string>());
    }

    // === Class-level tests (ported from ClassInterceptorBuilderTests) ===

    [Fact]
    public Task Emit_SimpleClass_GeneratesCorrectly()
    {
        var model = MakeClass();
        return Verify(InterceptorEmitter.Emit(model), "cs");
    }

    [Fact]
    public Task Emit_WithOneMethod_GeneratesCorrectly()
    {
        var model = MakeClass(methods: new[] { MakeMethod() });
        return Verify(InterceptorEmitter.Emit(model), "cs");
    }

    [Fact]
    public Task Emit_WithTwoMethods_GeneratesCorrectly()
    {
        var model = MakeClass(methods: new[]
        {
            MakeMethod(name: "MyMethod1"),
            MakeMethod(name: "MyMethod2")
        });
        return Verify(InterceptorEmitter.Emit(model), "cs");
    }

    [Fact]
    public Task Emit_WithContext_GeneratesCorrectly()
    {
        var model = MakeClass(contextMembers: new[]
        {
            new ContextMemberModel("_balance", "double"),
            new ContextMemberModel("_name", "string")
        });
        return Verify(InterceptorEmitter.Emit(model), "cs");
    }

    [Fact]
    public Task Emit_WithGenerics_GeneratesCorrectly()
    {
        var model = MakeClass(typeParameters: "<T>", whereConstraints: "where T : class");
        return Verify(InterceptorEmitter.Emit(model), "cs");
    }

    [Theory]
    [InlineData(ClassSituation.Static)]
    [InlineData(ClassSituation.Unsafe)]
    [InlineData(ClassSituation.IsStruct)]
    [InlineData(ClassSituation.IsRefStruct)]
    public Task Emit_WithClassSituation_GeneratesCorrectly(ClassSituation situation)
    {
        var model = MakeClass(situation: situation);
        return Verify(InterceptorEmitter.Emit(model), "cs").UseParameters(situation);
    }

    // === Method-level tests (ported from MethodInterceptorBuilderTests) ===

    [Fact]
    public Task Emit_WithBaseMethod_GeneratesCorrectly()
    {
        var model = MakeClass(methods: new[] { MakeMethod() });
        return Verify(InterceptorEmitter.Emit(model), "cs");
    }

    [Fact]
    public Task Emit_StaticMethod_OnInstanceClass_HasNullContext()
    {
        var model = MakeClass(methods: new[] { MakeMethod(situation: MethodSituation.Static) });
        return Verify(InterceptorEmitter.Emit(model), "cs");
    }

    [Fact]
    public Task Emit_WithGenericMethod_GeneratesCorrectly()
    {
        var model = MakeClass(methods: new[]
        {
            MakeMethod(typeParameters: "<T>", whereConstraints: "where T : class")
        });
        return Verify(InterceptorEmitter.Emit(model), "cs");
    }

    [Fact]
    public Task Emit_WithGenericClass_GeneratesCorrectly()
    {
        var model = MakeClass(typeParameters: "<T>", methods: new[] { MakeMethod() });
        return Verify(InterceptorEmitter.Emit(model), "cs");
    }

    [Theory]
    [InlineData(MethodSituation.Async)]
    [InlineData(MethodSituation.Static)]
    [InlineData(MethodSituation.Unsafe)]
    [InlineData(MethodSituation.ReturnsRef)]
    [InlineData(MethodSituation.ReturnsRefReadonly)]
    public Task Emit_WithMethodSituation_GeneratesCorrectly(MethodSituation situation)
    {
        var model = MakeClass(methods: new[] { MakeMethod(situation: situation) });
        return Verify(InterceptorEmitter.Emit(model), "cs").UseParameters(situation);
    }

    [Theory]
    [InlineData(ClassSituation.Static)]
    [InlineData(ClassSituation.Unsafe)]
    [InlineData(ClassSituation.IsStruct)]
    [InlineData(ClassSituation.IsRefStruct)]
    public Task Emit_MethodWithClassSituation_GeneratesCorrectly(ClassSituation situation)
    {
        var model = MakeClass(situation: situation, methods: new[] { MakeMethod() });
        return Verify(InterceptorEmitter.Emit(model), "cs").UseParameters(situation);
    }
}
