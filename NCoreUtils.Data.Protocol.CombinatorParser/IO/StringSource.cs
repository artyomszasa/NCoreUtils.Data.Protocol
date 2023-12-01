using System;

namespace NCoreUtils.Data.Protocol.IO;

public ref struct StringSource(string source)
{
    public ReadOnlySpan<char> Source { get; } = source.AsSpan();

    public int Position { get; private set; }

    public readonly ReadOnlySpan<char> Pending
        => Source[Position..];

    public readonly bool Eos => Position >= Source.Length;

    public void Advance(int count)
        => Position += count;
}