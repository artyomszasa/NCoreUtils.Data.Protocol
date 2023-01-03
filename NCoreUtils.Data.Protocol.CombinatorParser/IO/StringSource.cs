using System;

namespace NCoreUtils.Data.Protocol.IO;

public ref struct StringSource
{
    public ReadOnlySpan<char> Source { get; }

    public int Position { get; private set; }

    public ReadOnlySpan<char> Pending
        => Source[Position..];

    public bool Eos => Position >= Source.Length;

    public StringSource(string source)
        => Source = source.AsSpan();

    public void Advance(int count)
        => Position += count;
}