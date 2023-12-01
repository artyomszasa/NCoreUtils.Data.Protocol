using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.TypeInference;

public class DefaultPropertyResolver : IPropertyResolver
{
    private readonly struct TypeAndPropertyName(Type type, string propertyName) : IEquatable<TypeAndPropertyName>
    {
        public Type Type { get; } = type;

        public string PropertyName { get; } = propertyName;

        public bool Equals(TypeAndPropertyName other)
            => Type == other.Type && PropertyName == other.PropertyName;

        public override bool Equals([NotNullWhen(true)] object? obj)
            => obj is TypeAndPropertyName other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(Type, PropertyName);
    }

    private static ConcurrentDictionary<IDataUtils, DefaultPropertyResolver>? _instanceCache;

    private static Func<IDataUtils, DefaultPropertyResolver>? _instanceFactory;

    public static ConcurrentDictionary<IDataUtils, DefaultPropertyResolver> InstanceCache
        => _instanceCache ??= new();

    public static Func<IDataUtils, DefaultPropertyResolver> InstanceFactory
        => _instanceFactory ??= static util => new(util);

    public static DefaultPropertyResolver For(IDataUtils util)
        => InstanceCache.GetOrAdd(util, InstanceFactory);

    private ConcurrentDictionary<TypeAndPropertyName, IProperty> Cache { get; } = new();

    public IDataUtils Util { get; }

    private DefaultPropertyResolver(IDataUtils util)
        => Util = util ?? throw new ArgumentNullException(nameof(util));

    public bool TryResolveProperty(
        Type instanceType,
        string propertyName,
        [MaybeNullWhen(false)] out IProperty property)
    {
        if (Cache.TryGetValue(new(instanceType, propertyName), out var prop))
        {
            property = prop;
            return true;
        }
        if (Util.TryGetProperty(instanceType, propertyName, out var propertyInfo))
        {
            property = Cache.GetOrAdd(new(instanceType, propertyName), new DefaultProperty(propertyInfo));
            return true;
        }
        property = default;
        return false;
    }
}