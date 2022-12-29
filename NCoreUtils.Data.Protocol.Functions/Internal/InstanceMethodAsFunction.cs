using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.Internal;

public class InstanceMethodAsFunction : IFunction
{
    public InstanceMethodAsFunctionDescriptor Descriptor { get; }

    public InstanceMethodAsFunction(InstanceMethodAsFunctionDescriptor descriptor)
        => Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));

    public FunctionMatch MatchFunction(IDataUtils utils, Expression expression)
    {
        if (expression is MethodCallExpression call && call.Method == Descriptor.Method)
        {
            return new(
                Descriptor.Name,
                call.Arguments.Prepend(call.Object!).ToList()
            );
        }
        return default;
    }

    public bool TryResolveFunction(
        IDataUtils util,
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        [MaybeNullWhen(false)] out IFunctionDescriptor descriptor)
    {
        if (!StringComparer.InvariantCultureIgnoreCase.Equals(name, Descriptor.Name))
        {
            descriptor = default;
            return false;
        }
        if (!resultTypeConstraints.IsCompatible(Descriptor.ResultType, util))
        {
            descriptor = default;
            return false;
        }
        if (argumentTypeConstraints.Count != Descriptor.ArgumentTypes.Count)
        {
            descriptor = default;
            return false;
        }
        for (var i = 0; i < argumentTypeConstraints.Count; ++i)
        {
            var constraints = argumentTypeConstraints[i];
            var type = Descriptor.ArgumentTypes[i];
            if (!constraints.IsCompatible(type, util))
            {
                descriptor = default;
                return false;
            }
        }
        descriptor = Descriptor;
        return true;
    }
}