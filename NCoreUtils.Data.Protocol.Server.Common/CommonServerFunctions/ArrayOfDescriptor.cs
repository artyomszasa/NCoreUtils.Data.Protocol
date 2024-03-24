using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using NCoreUtils.Data.Protocol.TypeInference;
using Names = NCoreUtils.Data.Protocol.CommonFunctionNames;

namespace NCoreUtils.Data.Protocol.CommonServerFunctions;

internal sealed class ArrayOfDescriptor : IFunctionDescriptor
{
    public Type ElementType { get; }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type ResultType {  get; }

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

    [UnconditionalSuppressMessage("Trimming", "IL3050", Justification = "Array type is preserved in constructor.")]
    public Expression CreateExpression(IReadOnlyList<Expression> arguments)
        => Expression.NewArrayInit(ElementType, arguments);
}