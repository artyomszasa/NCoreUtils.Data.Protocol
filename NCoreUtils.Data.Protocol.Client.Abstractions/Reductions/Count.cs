namespace NCoreUtils.Data.Protocol.Reductions;

public sealed class Count : Reduction
{
    public static Count Singleton { get; } = new();

    public override string Name => "count";

    public override bool AllowNull => false;

    private Count() { }
}