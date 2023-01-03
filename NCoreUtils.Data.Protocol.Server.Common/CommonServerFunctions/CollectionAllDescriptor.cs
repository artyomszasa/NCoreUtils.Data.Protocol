using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.TypeInference;
using Names = NCoreUtils.Data.Protocol.CommonFunctionNames;

namespace NCoreUtils.Data.Protocol.CommonServerFunctions;

internal sealed class CollectionAllDescriptor : IFunctionDescriptor
{
    public MethodInfo MethodAll { get; }

    public Type ResultType
    {
        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        get => typeof(bool);
    }

    public ReadOnlyConstraintedTypeList ArgumentTypes { get; }

    public string Name
    {
        [ExcludeFromCodeCoverage]
        get => Names.Every;
    }

    public CollectionAllDescriptor(
        MethodInfo methodAll,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type enumerableType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type lambdaType)
    {
        MethodAll = methodAll ?? throw new ArgumentNullException(nameof(methodAll));
        ArgumentTypes = new ReadOnlyConstraintedTypeListBuilder
        {
            enumerableType,
            lambdaType
        }.Build();
    }

    public Expression CreateExpression(IReadOnlyList<Expression> arguments)
        => Expression.Call(MethodAll, arguments);
}
