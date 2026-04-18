using System.Diagnostics.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.Generator;

internal sealed class ProtocolContextTargetV2(
    string @namespace,
    string name,
    HashSet<ITypeData> types,
    Dictionary<string, SomeType> explicitDescriptors,
    HashSet<(SomeType Arg, SomeType Res)> lambdaTypes)
    : IEquatable<ProtocolContextTargetV2>
{
    public string Namespace { get; } = @namespace;

    public string Name { get; } = name;

    public HashSet<ITypeData> Types { get; } = types;

    public IReadOnlyDictionary<string, SomeType> ExplicitDescriptors { get; } = explicitDescriptors;

    public HashSet<(SomeType Arg, SomeType Res)> LambdaTypes { get; } = lambdaTypes;

    public bool Equals([NotNullWhen(true)] ProtocolContextTargetV2? other)
    {
        if (other is null)
        {
            return false;
        }
        if (Name != other.Name || Namespace != other.Namespace || !Types.SetEquals(other.Types) || !LambdaTypes.SetEquals(other.LambdaTypes))
        {
            return false;
        }
        // dict equality
        if (ExplicitDescriptors.Count != other.ExplicitDescriptors.Count)
        {
            return false;
        }
        foreach (var kv in ExplicitDescriptors)
        {
            var key = kv.Key;
            var thisValue = kv.Value;
            if (!other.ExplicitDescriptors.TryGetValue(key, out var otherValue) || thisValue != otherValue)
            {
                return false;
            }
        }
        return true;
    }
}
