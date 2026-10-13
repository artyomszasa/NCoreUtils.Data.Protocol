using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NCoreUtils.Data.Protocol.Generator;

internal readonly struct SomeType
    : IEquatable<SomeType>
{
    public sealed class TypeNameEqualityComparer
        : IEqualityComparer<SomeType>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Eq(SomeType x, SomeType y)
            => StringComparer.Ordinal.Equals(x.FullName, y.FullName);

        public bool Equals(SomeType x, SomeType y)
            => Eq(x, y);

        public int GetHashCode(SomeType obj)
            => StringComparer.Ordinal.GetHashCode(obj.FullName);

        public override bool Equals(object obj)
            => obj is TypeNameEqualityComparer;

        public override int GetHashCode()
            => default;
    }

    public static TypeNameEqualityComparer ByNameEqualityComparer { get; } = new();

    public static bool operator==(SomeType a, SomeType b)
        => a.Equals(b);

    public static bool operator!=(SomeType a, SomeType b)
        => !a.Equals(b);

    private readonly ITypeData? _data;

    private readonly string? _fullName;

    public string FullName => _fullName ?? _data?.FullName ?? throw new InvalidOperationException();

    public TypeSyntax TypeName => _data is { TypeName: var typeName }
        ? typeName
        : SyntaxFactory.ParseTypeName(_fullName ?? throw new InvalidOperationException());

    private SomeType(ITypeData? data, string? fullName)
    {
        _data = data;
        _fullName = fullName;
    }

    public SomeType(ITypeData data) : this(data.ThrowIfNull(), default) { }

    public SomeType(string fullName) : this(null, fullName.ThrowIfNull()) { }

    public bool Equals(SomeType other)
    {
        if (_data is ITypeData data)
        {
            return other._data is ITypeData otherData && data.Equals(otherData);
        }
        if (_fullName is string fullName)
        {
            return other._fullName is string otherFullName && fullName == otherFullName;
        }
        return other._data is null && other._fullName is null;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is SomeType other && Equals(other);

    public override int GetHashCode()
    {
        if (_data is ITypeData data)
        {
            return unchecked((int)(0x00000001u | (uint)data.GetHashCode()));
        }
        if (_fullName is string fullName)
        {
            return unchecked((int)(0xFFFFFFFFEu & (uint)fullName.GetHashCode()));
        }
        return default;
    }

    public override string ToString()
        => _data is null ? $"explicit:{FullName}" : $"type:{FullName}";
}
