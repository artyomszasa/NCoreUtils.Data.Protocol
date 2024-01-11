using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace NCoreUtils.Data.Protocol.Internal;

public static class EnumParserHelper
{
    public ref struct FlagEnumerator
    {
        private readonly ReadOnlySpan<char> Source;

        private int Offset;

        public ReadOnlySpan<char> Current { get; private set; }

        public FlagEnumerator(ReadOnlySpan<char> source)
        {
            Source = source;
            Offset = 0;
            Current = ReadOnlySpan<char>.Empty;
        }

        public bool MoveNext()
        {
            if (Offset >= Source.Length) { return false; }
            // handle leading spaces
            while (Offset < Source.Length && Source[Offset] == ' ') { ++Offset; }
            if (Offset >= Source.Length) { return false; }
            // parse single value
            var start = Offset;
            while (Offset < Source.Length && !(Source[Offset] is ' ' or ',' or '|')) { ++Offset; }
            if (Offset == start) { throw new FormatException($"Unable to parse \"{Source.ToString()}\" as enum flags."); }
            var end = Offset;
            // handle trailing spaces before pipe or comma
            while (Offset < Source.Length && Source[Offset] == ' ') { ++Offset; }
            // handle pipe or eos
            if (Offset < Source.Length)
            {
                if (Source[Offset] is ',' or '|')
                {
                    ++Offset;
                }
                else
                {
                    throw new FormatException($"Unable to parse \"{Source.ToString()}\" as enum flags.");
                }
            }
            Current = end >= Source.Length ? Source[start..] : Source[start..end];
            return true;
        }
    }

    public readonly ref struct FlagEnumerable
    {
        private readonly ReadOnlySpan<char> Source;

        public FlagEnumerable(ReadOnlySpan<char> source)
            => Source = source;

        public FlagEnumerator GetEnumerator()
            => new(Source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FlagEnumerable EnumerateFlags(string source)
        => new(source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSame(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        => MemoryExtensions.Equals(a, b, StringComparison.InvariantCultureIgnoreCase);
}