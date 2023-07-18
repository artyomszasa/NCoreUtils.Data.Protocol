using System.Runtime.CompilerServices;

namespace NCoreUtils.Data.Protocol.Lexing;

internal static class CharUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLetter(char c)
        => (uint)((c | 0x20) - 'a') <= ('z' - 'a');

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDigit(char ch)
        => (uint)(ch - '0') <= ('9' - '0');

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLetterOrDigit(char ch)
        => IsLetter(ch) || IsDigit(ch);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLetterOrUnderscore(char ch)
        => IsLetter(ch) || ch == '_';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLetterOrDigitOrUnderscore(char ch)
        => IsLetterOrDigit(ch) || ch == '_';
}