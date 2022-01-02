using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.Internal;

public sealed class PropertyAsFunction : IFunction
{
    public PropertyAsFunctionDescriptor Descriptor { get; }

    public PropertyAsFunction(PropertyAsFunctionDescriptor descriptor)
        => Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));

    public bool TryResolveFunction(
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        [MaybeNullWhen(false)] out IFunctionDescriptor descriptor)
    {
        if (StringComparer.InvariantCultureIgnoreCase.Equals(name, Descriptor.Name)
            && argumentTypeConstraints.Count == 1
            && argumentTypeConstraints[0].IsCompatible(Descriptor.ArgumentTypes[0])
            && resultTypeConstraints.IsCompatible(Descriptor.ResultType))
        {
            descriptor = Descriptor;
            return true;
        }
        descriptor = default;
        return false;
    }

    public FunctionMatch MatchFunction(Expression expression)
    {
        if (expression is MemberExpression m
            && m.Expression is not null
            && m.Member is PropertyInfo p
            && p.DeclaringType == Descriptor.Property.DeclaringType
            && p.Name == Descriptor.Property.Name)
        {
            return new FunctionMatch(Descriptor.Name, new[] { m.Expression });
        }
        return default;
    }
}