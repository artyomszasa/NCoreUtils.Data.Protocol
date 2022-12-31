namespace NCoreUtils.Data.Protocol.Generator;

internal struct IntDesc
{
    public int Size { get; }

    public bool Signed { get; }

    public bool Nullable { get; }

    public IntDesc(int size, bool signed, bool nullable)
    {
        Size = size;
        Signed = signed;
        Nullable = nullable;
    }
}