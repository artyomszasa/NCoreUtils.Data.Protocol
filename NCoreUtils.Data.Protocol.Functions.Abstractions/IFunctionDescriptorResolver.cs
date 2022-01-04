using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol;

public interface IFunctionDescriptorResolver
{
    /// <summary>
    /// Attempt to resolve function based on the specified parameters.
    /// </summary>
    /// <param name="name">Function name.</param>
    /// <param name="resultTypeConstraints">Deducted type or type constraints of the return type.</param>
    /// <param name="argumentTypeConstraints">Deducted types or type constraints of the argument types.</param>
    /// <param name="descriptor">Stores function descriptor if the resolvation is successfull.</param>
    /// <returns>
    /// <c>true</c> if the function has been resolved, <c>false</c> otherwise.
    /// </returns>
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