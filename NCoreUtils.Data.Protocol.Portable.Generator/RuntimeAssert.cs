using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NCoreUtils.Data.Protocol.Generator;

internal static class RuntimeAssert
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static T ThrowIfNull<T>([NotNull] this T? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        where T : class
    {
        if (argument is null)
        {
            throw new ArgumentNullException(paramName);
        }
        return argument;
    }
}