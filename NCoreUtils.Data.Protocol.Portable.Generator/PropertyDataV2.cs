using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NCoreUtils.Data.Protocol.Generator;

internal sealed class PropertyDataV2(string name, string typeFullName)
    : IEquatable<PropertyDataV2>
{
    public static bool operator==(PropertyDataV2? a, PropertyDataV2? b)
        => a is null
            ? b is null
            : a.Equals(b);

    public static bool operator!=(PropertyDataV2? a, PropertyDataV2? b)
        => a is null
            ? b is not null
            : !a.Equals(b);

    private TypeSyntax? _typeName;

    public string Name { get; } = name;

    public string TypeFullName { get; } = typeFullName;

    public TypeSyntax TypeName => _typeName ??= SyntaxFactory.ParseTypeName(TypeFullName);

    public bool Equals([NotNullWhen(true)] PropertyDataV2? other)
        => ReferenceEquals(this, other)
            || (other is not null
                && Name == other.Name
                && TypeFullName == other.TypeFullName);

    public override bool Equals([MaybeNullWhen(false)] object? obj)
        => Equals(obj as PropertyDataV2);

    public override int GetHashCode()
        => StringComparer.Ordinal.GetHashCode(Name) ^ StringComparer.Ordinal.GetHashCode(TypeFullName);
}
