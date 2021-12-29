using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol;

public interface IFunctionDescriptorResolver
{
    bool TryResolveFunction(
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        [MaybeNullWhen(false)] out IFunctionDescriptor descriptor
    );

    IFunctionDescriptor ResolveFunction(
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        Func<IFunctionDescriptor> next)
        => TryResolveFunction(name, resultTypeConstraints, argumentTypeConstraints, out var descriptor)
            ? descriptor
            : next();
}