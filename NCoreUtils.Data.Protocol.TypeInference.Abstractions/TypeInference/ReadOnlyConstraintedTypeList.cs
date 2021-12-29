using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.TypeInference;

public class ReadOnlyConstraintedTypeList : IReadOnlyList<Type>
{
    private readonly IReadOnlyList<Type> _types;

    Type IReadOnlyList<Type>.this[int index] => this[index];

    public Type this[int index]
    {
        [UnconditionalSuppressMessage("Trimming", "IL2073", Justification = "Builder has necessary attributes.")]
        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        get => _types[index];
    }

    public int Count => _types.Count;

    internal ReadOnlyConstraintedTypeList(IReadOnlyList<Type> types)
        => _types = types ?? throw new ArgumentNullException(nameof(types));

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public IEnumerator<Type> GetEnumerator()
        => _types.GetEnumerator();
}