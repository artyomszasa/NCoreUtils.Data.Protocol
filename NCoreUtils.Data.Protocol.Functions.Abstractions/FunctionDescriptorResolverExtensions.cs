using System;
using System.Collections.Generic;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol;

public static class FunctionDescriptorResolverExtensions
{
    private static Func<IFunctionDescriptor> RetNull { get; } = () => default!;

    public static IFunctionDescriptor? ResolveFunction(
        this IFunctionDescriptorResolver resolver,
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints)
        => resolver.ResolveFunction(name, resultTypeConstraints, argumentTypeConstraints, RetNull);
}