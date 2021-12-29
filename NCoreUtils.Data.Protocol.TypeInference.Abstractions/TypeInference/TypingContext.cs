namespace NCoreUtils.Data.Protocol.TypeInference;

public ref struct TypingContext
{
    private int _supply = 0;

    public TypeUid CreateTypeUid()
    {
        var uid = new TypeUid(_supply);
        ++_supply;
        return uid;
    }
}