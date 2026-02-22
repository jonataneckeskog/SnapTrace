using Microsoft.CodeAnalysis;
using SnapTrace.Generators.Builders;
using SnapTrace.Generators.Definitions;

namespace SnapTrace.Generators.Processing;

internal static class SnapTraceEmitter
{
    public static void Emit(SourceProductionContext context, ClassToGenerate classToGenerate)
    {
        var sourceFileBuilder = new SourceFileBuilder();
        sourceFileBuilder.WithNamespace(classToGenerate.@namespace, nsBuilder =>
        {
            nsBuilder.WithClass(classToGenerate.className, classToGenerate.situation, classBuilder =>
            {
                if (classToGenerate.typeParameters is not null)
                {
                    classBuilder.WithTypeParameters(classToGenerate.typeParameters);
                }

                if (classToGenerate.whereConstraints is not null)
                {
                    classBuilder.WithWhereConstraints(classToGenerate.whereConstraints);
                }

                foreach (var member in classToGenerate.contextMembers)
                {
                    classBuilder.AddContextMember(member.Name, member.Type);
                }

                foreach (var method in classToGenerate.methods)
                {
                    classBuilder.WithMethod(method.Name, method.Situation, methodBuilder =>
                    {
                        if (method.GenericConstraints is not null)
                        {
                            methodBuilder.WithWhereConstraints(method.GenericConstraints);
                        }

                        foreach (var p in method.Parameters)
                        {
                            methodBuilder.WithParameter(p.Name, p.Type, p.Modifier, p.IsParams, p.DeepCopy, p.Redacted);
                        }

                        methodBuilder.WithReturn(method.ReturnDefinition.Type, method.ReturnDefinition.DeepCopy, method.ReturnDefinition.Redacted);
                    });
                }
            });
        });

        var sourceCode = sourceFileBuilder.Build();
        context.AddSource($"{classToGenerate.className}.g.cs", sourceCode);
    }
}
