namespace NCoreUtils.Data.Protocol.Reductions;

public class First : Reduction
{
    public static First Singleton { get; } = new();

    public override string Name => "first";

    public override bool AllowNull => false;

    internal First() { }
}

public sealed class FirstOrDefault : First
{
    public new static FirstOrDefault Singleton { get; } = new();

    public override bool AllowNull => true;

    private FirstOrDefault() { }
}