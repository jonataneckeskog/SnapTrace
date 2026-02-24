using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SnapTrace.Generators.Builders;

/// <summary>
/// Builders the individual source files for SnapTrace. SourceFileBuilder ensured the global
/// using 'SnapTrace' and then adds any amount of namespaces as needed.
/// </summary>
public class SourceFileBuilder
{
    private readonly Dictionary<string, NamespaceBuilder> _namespaces = new();

    public SourceFileBuilder WithNamespace(string namespaceName, Action<NamespaceBuilder> config)
    {
        if (!_namespaces.TryGetValue(namespaceName, out var nsBuilder))
        {
            nsBuilder = new NamespaceBuilder(namespaceName);
            _namespaces[namespaceName] = nsBuilder;
        }
        config(nsBuilder);

        return this;
    }

    /// <summary>
    /// Builds the full file, using a chained method to the underlying namespace, class and method builders.
    /// </summary>
    /// <returns></returns>
    public string Build()
    {
        StringBuilder sb = new();

        using (var stringWriter = new StringWriter(sb))
        using (var writer = new IndentedTextWriter(stringWriter, "    "))
        {
            // 1. Global Usings
            writer.WriteLine("#nullable enable");
            writer.WriteLine("using global::SnapTrace;");

            // 2. Iterate through namespaces
            var namespacesList = _namespaces.Values.ToList();
            foreach (var __namespace in namespacesList)
            {
                writer.WriteLine();
                __namespace.InternalBuild(writer);
            }
        }

        return sb.ToString();
    }
}
