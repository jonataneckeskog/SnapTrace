namespace SnapTrace.Generators.Constants;

internal static class GeneratorUtils
{
    public const string SnapCloner = @"
#nullable enable
namespace SnapTrace.Generated
{
    internal static class SnapCloner
    {
        private static readonly System.Reflection.MethodInfo CloneMethod = 
            typeof(object).GetMethod(""MemberwiseClone"", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        public static object? Clone(object? obj)
        {
            if (obj == null) return null;
            var type = obj.GetType();
            
            // Handle Tuples (System.Runtime.CompilerServices.ITuple)
            if (obj is System.Runtime.CompilerServices.ITuple tuple)
            {
                // We create a generic object array to hold the 'cloned' parts of the tuple
                var elements = new object?[tuple.Length];
                for (int i = 0; i < tuple.Length; i++)
                {
                    elements[i] = Clone(tuple[i]); // Recursively clone elements
                }
                return elements; // Store as an array for the trace record
            }

            if (type.IsValueType || obj is string) return obj; 

            return CloneMethod.Invoke(obj, null);
        }
    }
}";
}