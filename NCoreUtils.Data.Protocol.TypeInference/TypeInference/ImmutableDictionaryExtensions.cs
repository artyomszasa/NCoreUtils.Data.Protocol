using System;
using System.Collections.Immutable;

namespace NCoreUtils.Data.Protocol.TypeInference;

internal static class ImmutableDictionaryExtensions
{
    public static ImmutableDictionary<TKey, TValue> UpdateItem<TKey, TValue>(
        this ImmutableDictionary<TKey, TValue> source,
        TKey key,
        Func<TValue, TValue> mapping)
        where TKey : notnull
    {
        if (source.TryGetValue(key, out var value))
        {
            return source.SetItem(key, mapping(value));
        }
        return source;
    }
}