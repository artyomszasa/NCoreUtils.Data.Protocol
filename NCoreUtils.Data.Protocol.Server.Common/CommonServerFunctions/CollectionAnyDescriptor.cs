using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.TypeInference;
using Names = NCoreUtils.Data.Protocol.CommonFunctionNames;

namespace NCoreUtils.Data.Protocol.CommonServerFunctions;

internal sealed class CollectionAnyDescriptor : IFunctionDescriptor
{
    public MethodInfo MethodAny { get; }

    public Type ResultType
    {
        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        get => typeof(bool);
    }

    public ReadOnlyConstraintedTypeList ArgumentTypes { get; }

    public string Name
    {
        [ExcludeFromCodeCoverage]
        get => Names.Some;
    }

    public CollectionAnyDescriptor(
        MethodInfo methodAny,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type enumerableType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type lambdaType)
    {
        MethodAny = methodAny ?? throw new ArgumentNullException(nameof(methodAny));
        ArgumentTypes = new ReadOnlyConstraintedTypeListBuilder
        {
            enumerableType,
            lambdaType
        }.Build();
    }

    public Expression CreateExpression(IReadOnlyList<Expression> arguments)
        => Expression.Call(MethodAny, arguments);
}
