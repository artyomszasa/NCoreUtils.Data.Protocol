using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public abstract class ArrayOfDescriptor : IFunctionDescriptor
{
    public abstract Type ElementType { get; }

    public abstract Type ResultType { get; }

    public abstract ReadOnlyConstraintedTypeList ArgumentTypes { get; }

    public string Name => Names.Array;

    internal ArrayOfDescriptor() { }

    public Expression CreateExpression(IReadOnlyList<Expression> arguments)
        => Expression.NewArrayInit(ElementType, arguments);
}

public sealed class ArrayOfDescriptor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : ArrayOfDescriptor
{
    public override Type ElementType => typeof(T);

    public override Type ResultType => typeof(T[]);

    public override ReadOnlyConstraintedTypeList ArgumentTypes { get; }

    public ArrayOfDescriptor(int count)
    {
        var builder = new ReadOnlyConstraintedTypeListBuilder(count);
        for (var i = 0; i < count; ++i)
        {
            builder.Add(typeof(T));
        }
        ArgumentTypes = builder.Build();
    }
}