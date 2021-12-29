using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.TypeInference;

public class DefaultPropertyResolver : IPropertyResolver
{
    private struct TypeAndPropertyName : IEquatable<TypeAndPropertyName>
    {
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
        public Type Type { get; }

        public string PropertyName { get; }

        public TypeAndPropertyName([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type, string propertyName)
        {
            Type = type;
            PropertyName = propertyName;
        }

        public bool Equals(TypeAndPropertyName other)
            => Type == other.Type && PropertyName == other.PropertyName;

        public override bool Equals([NotNullWhen(true)] object? obj)
            => obj is TypeAndPropertyName other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(Type, PropertyName);
    }

    private static ConcurrentDictionary<TypeAndPropertyName, IProperty?> Cache { get; } = new();

    private static Func<TypeAndPropertyName, IProperty?> Factory { get; } = GetProperty;

    public static DefaultPropertyResolver Singleton { get; } = new DefaultPropertyResolver();

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Argument type has proper annotations in TryResolveProperty.")]
    private static IProperty? GetProperty(TypeAndPropertyName args)
    {
        var prop = args.Type.GetProperty(args.PropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy);
        return prop is null ? default : new DefaultProperty(prop);
    }

    private DefaultPropertyResolver() { }

    public bool TryResolveProperty(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type instanceType,
        string propertyName,
        [MaybeNullWhen(false)] out IProperty property)
    {
        var prop = Cache.GetOrAdd(new(instanceType, propertyName), Factory);
        if (prop is null)
        {
            property = default;
            return false;
        }
        property = prop;
        return true;
    }
}