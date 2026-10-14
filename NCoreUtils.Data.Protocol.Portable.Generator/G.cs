using System.Runtime.CompilerServices;

namespace NCoreUtils.Data.Protocol.Generator;

internal static class G
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Eq(string? a, string? b)
        => StringComparer.Ordinal.Equals(a, b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Eq(bool a, bool b)
        => a == b;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Eq(IReadOnlyList<PropertyDataV2>? a, IReadOnlyList<PropertyDataV2>? b)
    {
        if (a is null)
        {
            return b is null;
        }
        if (b is null)
        {
            return false;
        }
        return a.SequenceEqual(b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Eq(IReadOnlyList<EnumFieldData>? a, IReadOnlyList<EnumFieldData>? b)
    {
        if (a is null)
        {
            return b is null;
        }
        if (b is null)
        {
            return false;
        }
        return a.SequenceEqual(b);
    }
}