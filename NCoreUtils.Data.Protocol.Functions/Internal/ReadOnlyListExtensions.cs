using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NCoreUtils.Data.Protocol.Internal;

public static class ReadOnlyListExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IReadOnlyList<T> SliceNoNullCheck<T>(this IReadOnlyList<T> source, int start, int length)
    {
        if (start + length > source.Count)
        {
            throw new ArgumentException("Invalid range.");
        }
        var result = new T[length];
        for (var i = 0; i < length; ++i)
        {
            result[i] = source[i + start];
        }
        return result;
    }

    public static IReadOnlyList<T> Slice<T>(this IReadOnlyList<T> source, int start, int length)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        return source.SliceNoNullCheck(start, length);
    }

    public static IReadOnlyList<T> Slice<T>(this IReadOnlyList<T> source, int start)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        return source.SliceNoNullCheck(start, source.Count - start);
    }
}