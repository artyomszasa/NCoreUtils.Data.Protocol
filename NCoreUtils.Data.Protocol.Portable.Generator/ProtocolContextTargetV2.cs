using System.Diagnostics.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.Generator;

internal sealed class ProtocolContextTargetV2(
    string @namespace,
    string name,
    HashSet<ITypeData> types,
    HashSet<(SomeType Arg, SomeType Res)> lambdaTypes)
    : IEquatable<ProtocolContextTargetV2>
{
    public static bool operator ==(ProtocolContextTargetV2? a, ProtocolContextTargetV2? b)
        => a is null
            ? b is null
            : a.Equals(b);

    public static bool operator !=(ProtocolContextTargetV2? a, ProtocolContextTargetV2? b)
        => !(a == b);

    public string Namespace { get; } = @namespace;

    public string Name { get; } = name;

    public HashSet<ITypeData> Types { get; } = types;

    public HashSet<(SomeType Arg, SomeType Res)> LambdaTypes { get; } = lambdaTypes;

    public bool Equals([NotNullWhen(true)] ProtocolContextTargetV2? other)
    {
        if (other is null)
        {
            return false;
        }
        return Name == other.Name
            && Namespace == other.Namespace
            && Types.SetEquals(other.Types)
            && LambdaTypes.SetEquals(other.LambdaTypes);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
        => Equals(obj as ProtocolContextTargetV2);

    public override int GetHashCode()
        => (StringComparer.OrdinalIgnoreCase.GetHashCode(Name) + 28)
            ^ StringComparer.OrdinalIgnoreCase.GetHashCode(Namespace);
}
