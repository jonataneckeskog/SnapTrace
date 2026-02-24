using System;

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }

#pragma warning disable CS9113 // Unread parameter
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    file sealed class InterceptsLocationAttribute(string filePath, int line, int character) : Attribute;
#pragma warning restore CS9113 // Unread parameter
}

namespace Microsoft.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    file sealed class InterceptableAttribute : Attribute;
}
