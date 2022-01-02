using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.Internal;

public sealed class InstanceMethodAsFunctionDescriptor : IFunctionDescriptor
{
    public MethodInfo Method { get; }

    public Type ResultType
    {
        [UnconditionalSuppressMessage("Trimmer", "IL2073", Justification = "handled in ctor.")]
        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        get => Method.ReturnType;
    }

    public ReadOnlyConstraintedTypeList ArgumentTypes { get; }

    public string Name { get; }

    [RequiresUnreferencedCode("All affected types must be preserved by caller.")]
    public InstanceMethodAsFunctionDescriptor(MethodInfo method, Type? instanceType, string name)
    {
        var parameters = method.GetParameters();
        var argumentTypes = new ReadOnlyConstraintedTypeListBuilder(parameters.Length + 1)
        {
            instanceType
                ?? method.DeclaringType
                ?? throw new InvalidOperationException("Instance argument type cannot be deducted and must be specified explicitly.")
        };
        for (var i = 0; i < parameters.Length; ++i)
        {
            var parameter = parameters[i];
            if (parameter.IsIn || parameter.IsOut || parameter.ParameterType.IsByRef)
            {
                throw new InvalidOperationException($"By-ref parameters are not supported (parameter at {i} -> {parameter}).");
            }
            argumentTypes.Add(parameter.ParameterType);
        }
        Method = method;
        ArgumentTypes = argumentTypes.Build();
        Name = name;
    }

    public Expression CreateExpression(IReadOnlyList<Expression> arguments)
        => Expression.Call(
            arguments[0],
            Method,
            arguments.Slice(1, arguments.Count - 1)
        );
}