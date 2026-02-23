namespace SnapTrace.Generators.Models;

/// <summary>
/// Model 'collected' by the generator and later send to Builders
/// to handle building the syntax.
/// </summary>
/// <param name="Name"></param>
/// <param name="Type"></param>
internal record ContextMemberData(string Name, string Type);
