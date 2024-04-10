namespace NCoreUtils.Data.Protocol.Reductions;

public class Last : Reduction
{
    public static Last Singleton { get; } = new();

    public override string Name => "last";

    public override bool AllowNull => false;

    internal Last() { }
}

public sealed class LastOrDefault : Last
{
    public new static LastOrDefault Singleton { get; } = new();

    public override bool AllowNull => true;

    private LastOrDefault() { }
}