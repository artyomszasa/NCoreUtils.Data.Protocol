namespace NCoreUtils.Data.Protocol.Reductions;

public sealed class Any : Reduction
{
    public static Any Singleton { get; } = new();

    public override string Name => "any";

    public override bool AllowNull => false;

    private Any() { }
}