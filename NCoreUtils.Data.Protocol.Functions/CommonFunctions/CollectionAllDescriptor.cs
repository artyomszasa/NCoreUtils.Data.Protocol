using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public sealed class CollectionAllDescriptor : IFunctionDescriptor
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

    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "Preserved by caller.")]
    internal CollectionAllDescriptor(MethodInfo methodAll)
    {
        MethodAll = methodAll ?? throw new ArgumentNullException(nameof(methodAll));
        var parameters = methodAll.GetParameters();
        ArgumentTypes = new ReadOnlyConstraintedTypeListBuilder()
            .Add(parameters[0].ParameterType)
            .Add(parameters[1].ParameterType)
            .Build();
    }

    public Expression CreateExpression(IReadOnlyList<Expression> arguments)
        => Expression.Call(MethodAll, arguments);
}
