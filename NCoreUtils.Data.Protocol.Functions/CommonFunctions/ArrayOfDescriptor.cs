using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public sealed class ArrayOfDescriptor : IFunctionDescriptor
{
    public Type ElementType { get; }

    public Type ResultType { [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] get; }

    public ReadOnlyConstraintedTypeList ArgumentTypes { get; }

    public string Name => Names.Array;

    internal ArrayOfDescriptor(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type arrayType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type elementType,
        int count)
    {
        ElementType = elementType;
        ResultType = arrayType;
        var builder = new ReadOnlyConstraintedTypeListBuilder(count);
        for (var i = 0; i < count; ++i)
        {
            builder.Add(elementType);
        }
        ArgumentTypes = builder.Build();
    }

    public Expression CreateExpression(IReadOnlyList<Expression> arguments)
        => Expression.NewArrayInit(ElementType, arguments);
}