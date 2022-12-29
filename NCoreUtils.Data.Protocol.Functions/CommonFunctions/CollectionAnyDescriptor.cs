using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public sealed class CollectionAnyDescriptor : IFunctionDescriptor
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

    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "Preserved by caller.")]
    internal CollectionAnyDescriptor(MethodInfo methodAny)
    {
        MethodAny = methodAny ?? throw new ArgumentNullException(nameof(methodAny));
        var parameters = methodAny.GetParameters();
        ArgumentTypes = new ReadOnlyConstraintedTypeListBuilder()
            .Add(parameters[0].ParameterType)
            .Add(parameters[1].ParameterType)
            .Build();
    }

    public Expression CreateExpression(IReadOnlyList<Expression> arguments)
        => Expression.Call(MethodAny, arguments);
}