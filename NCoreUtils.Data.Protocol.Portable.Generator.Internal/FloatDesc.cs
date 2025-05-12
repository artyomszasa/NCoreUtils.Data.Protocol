namespace NCoreUtils.Data.Protocol.Generator;

internal readonly struct FloatDesc(int size, bool signed, bool nullable)
{
    public int Size { get; } = size;

    public bool Signed { get; } = signed;

    public bool Nullable { get; } = nullable;
}