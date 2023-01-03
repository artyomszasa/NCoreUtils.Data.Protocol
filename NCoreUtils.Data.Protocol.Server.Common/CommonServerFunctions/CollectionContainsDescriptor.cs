using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.TypeInference;
using Names = NCoreUtils.Data.Protocol.CommonFunctionNames;

namespace NCoreUtils.Data.Protocol.CommonServerFunctions;

internal sealed class CollectionContainsDescriptor : IFunctionDescriptor
{
    private MethodInfo MethodContains { get; }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type ResultType => typeof(bool);

    public ReadOnlyConstraintedTypeList ArgumentTypes { get; }

    public string Name => Names.Includes;

    public CollectionContainsDescriptor(
        MethodInfo methodContains,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type enumerableType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type elementType)
    {
        MethodContains = methodContains;
        ArgumentTypes = new ReadOnlyConstraintedTypeListBuilder
        {
            enumerableType,
            elementType
        }.Build();
    }

    public Expression CreateExpression(IReadOnlyList<Expression> arguments)
        => Expression.Call(MethodContains, arguments);
}