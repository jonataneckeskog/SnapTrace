namespace SnapTrace.Generators.Constants;

internal static class GeneratorUtils
{
    public const string SnapCloner = """
#nullable enable
namespace SnapTrace.Generated
{
    internal static class SnapCloner
    {
        private static readonly System.Reflection.MethodInfo? CloneMethod = 
            typeof(object).GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        public static object? Clone(object? obj)
        {
            if (obj == null) return null;
            
            // 1. Handle Tuples (ValueTuple)
            // Use global alias to ensure it works in any project
            if (obj is global::System.Runtime.CompilerServices.ITuple tuple)
            {
                var elements = new object?[tuple.Length];
                for (int i = 0; i < tuple.Length; i++)
                {
                    // Recursively clone elements in the tuple
                    elements[i] = Clone(tuple[i]);
                }
                return elements;
            }

            var type = obj.GetType();

            // 2. Simple Value Types & Strings
            // Note: We check this AFTER ITuple because Tuples are also ValueTypes
            if (type.IsValueType || obj is string) return obj;
            
            // 3. Perform shallow copy for Classes
            if (CloneMethod != null)
            {
                try 
                {
                    return CloneMethod.Invoke(obj, null);
                }
                catch 
                {
                    return obj?.ToString() ?? "Clone Error";
                }
            }
            
            return obj;
        }
    }
}
""";
}
