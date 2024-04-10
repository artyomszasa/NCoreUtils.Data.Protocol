namespace NCoreUtils.Data.Protocol.Reductions;

public class Single : Reduction
{
    public static Single Singleton { get; } = new();

    public override string Name => "single";

    public override bool AllowNull => false;

    internal Single() { }
}

public sealed class SingleOrDefault : First
{
    public new static SingleOrDefault Singleton { get; } = new();

    public override bool AllowNull => true;

    private SingleOrDefault() { }
}