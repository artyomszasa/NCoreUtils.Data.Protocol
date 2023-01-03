namespace NCoreUtils.Data.Protocol.TypeInference;

public static class TypeVariableExtensions
{
    public static TypeVariable Merge(this TypeVariable a, TypeVariable b, IDataUtils util)
    {
        if (a.IsResolved)
        {
            var atype = a.Type;
            if (b.IsResolved)
            {
                var btype = b.Type;
                return atype.Equals(btype)
                    ? a
                    : atype.IsAssignableFrom(btype)
                        ? b
                        : btype.IsAssignableFrom(atype)
                            ? a
                            : throw new ProtocolTypeInferenceException($"Incompatible types: {atype} {btype}");
            }
            return new(b.Constraints.Validate(util, atype));
        }
        if (b.IsResolved)
        {
            return new TypeVariable(a.Constraints.Validate(util, b.Type));
        }
        return new(b.Constraints.Merge(a.Constraints));
    }
}