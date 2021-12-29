using System;
using System.Diagnostics.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.TypeInference;

[Serializable]
public readonly struct TypeRef : IEquatable<TypeRef>
{
    public static bool operator ==(TypeRef left, TypeRef right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TypeRef left, TypeRef right)
    {
        return !(left == right);
    }

    public static implicit operator TypeRef(Type type)
        => new(type);

    private string TypeName { get; }

    public Type Type
    {
        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Type is bound in constructor.")]
        [UnconditionalSuppressMessage("Trimming", "IL2057", Justification = "Type is bound in constructor.")]
        get => Type.GetType(TypeName, true)!;
    }

    public TypeRef(Type type)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }
        TypeName = type.AssemblyQualifiedName ?? throw new InvalidOperationException($"Assembly qualified name is not accessible for {type}.");
    }

    public bool Equals(TypeRef other)
        => TypeName == other.TypeName;

    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is TypeRef other && Equals(other);

    public override int GetHashCode()
        => TypeName.GetHashCode();

    public override string ToString()
        => Type.Name;
}