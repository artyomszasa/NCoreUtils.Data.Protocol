using System;
using System.Collections.Immutable;

namespace NCoreUtils.Data.Protocol.Internal;

public static class TypeExtensions
{
    /*
    private abstract class NullableValueFactory : IOptionalValueFactory
    {
        private static ConcurrentDictionary<Type, NullableValueFactory>

        // public abstract object Create(object value, Type valueType);

        protected abstract object DoCreate(object value);
    }

    private sealed class NullableValueFactory<T> : NullableValueFactory
        where T : struct
    {
        protected override object DoCreate(object value)
            => new T?((T)value);
    }

    public interface IOptionalValueFactory
    {
        object Create(object value, Type valueType);
    }

    */

    private static ImmutableHashSet<Type> NullableTypeDefinitions { get; } = ImmutableHashSet.CreateRange(new []
    {
        typeof(Nullable<>),
        typeof(Maybe<>)
    });

    public static bool IsOptionalValue(this Type type)
        => type.IsGenericType && NullableTypeDefinitions.Contains(type.GetGenericTypeDefinition());
}