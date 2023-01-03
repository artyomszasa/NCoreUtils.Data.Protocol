using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.TypeInference;

public class ReadOnlyConstraintedTypeListBuilder : IEnumerable
{
    private List<Type> Types { get; }

    private ReadOnlyConstraintedTypeListBuilder(List<Type> types)
        => Types = types;

    public ReadOnlyConstraintedTypeListBuilder()
        : this(new List<Type>())
    { }

    public ReadOnlyConstraintedTypeListBuilder(int capacity)
        : this(new List<Type>(capacity))
    { }

    public ReadOnlyConstraintedTypeListBuilder Add(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type)
    {
        Types.Add(type);
        return this;
    }

    public ReadOnlyConstraintedTypeList Build()
        => new(Types);

    IEnumerator IEnumerable.GetEnumerator()
        => Types.GetEnumerator();
}