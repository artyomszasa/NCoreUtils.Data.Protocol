using System.Collections.Immutable;

namespace NCoreUtils.Data.Protocol.TypeInference;

public partial record TypeInferenceContext(
    ImmutableDictionary<TypeUid, TypeVariable> Types,
    ImmutableDictionary<TypeUid, ImmutableList<TypeUid>> Substitutions
);

public partial record TypeInferenceContext
{
    public static TypeInferenceContext Empty { get; } = new(
        ImmutableDictionary<TypeUid, TypeVariable>.Empty,
        ImmutableDictionary<TypeUid, ImmutableList<TypeUid>>.Empty
    );
}

