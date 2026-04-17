using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.Generator;

internal static class EnumerableeExtensions
{
    public static bool Any<TItem, TArg>(
        this ImmutableArray<TItem> source,
        TArg arg,
        Func<TItem, TArg, bool> predicate)
    {
        foreach (var item in source)
        {
            if (predicate(item, arg))
            {
                return true;
            }
        }
        return false;
    }

    public static bool TryDequeue<T>(this Queue<T> queue, [MaybeNullWhen(false)] out T item)
    {
        if (queue.Count > 0)
        {
            item = queue.Dequeue();
            return true;
        }
        item = default;
        return false;
    }

    public static bool TryDequeue<T1, T2>(
        this Queue<(T1, T2)> queue,
        [MaybeNullWhen(false)] out T1 item1,
        [MaybeNullWhen(false)] out T2 item2)
    {
        if (queue.TryDequeue(out var tup))
        {
            (item1, item2) = tup;
            return true;
        }
        (item1, item2) = (default, default);
        return false;
    }

    public static void Enqueue<T>(this Queue<(T, bool)> queue, T item, bool root = false)
        => queue.Enqueue((item, root));

    public static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, T value)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        yield return value;
        foreach (var item in source)
        {
            yield return item;
        }
    }
}