using NCoreUtils.Data.Protocol.Reductions;

namespace NCoreUtils.Data.Protocol;

public abstract class Reduction
{
    public static First First => First.Singleton;

    public static FirstOrDefault FirstOrDefault => FirstOrDefault.Singleton;

    public static Single Single => Single.Singleton;

    public static SingleOrDefault SingleOrDefault => SingleOrDefault.Singleton;

    public static Last Last => Last.Singleton;

    public static LastOrDefault LastOrDefault => LastOrDefault.Singleton;

    public static Count Count => Count.Singleton;

    public static Any Any => Any.Singleton;

    public abstract string Name { get; }

    public abstract bool AllowNull { get; }
}