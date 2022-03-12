using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.Internal;

/// <summary>
/// Hides disposablity of the underlying function descriptor resolver. Used when function matcher is disposable but
/// disposing is handled by the DI container.
/// </summary>
/// <typeparam name="T">Type of the underlying function descriptor resolver.</typeparam>
internal sealed class SuppressDisposeFunctionDescriptorResolver<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : IFunctionDescriptorResolver
    where T : class, IFunctionDescriptorResolver
{
    private T FunctionDescriptorResolver { get; }

    public SuppressDisposeFunctionDescriptorResolver(T functionDescriptorResolver)
        => FunctionDescriptorResolver = functionDescriptorResolver
            ?? throw new ArgumentNullException(nameof(functionDescriptorResolver));

    public bool TryResolveFunction(
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        [MaybeNullWhen(false)] out IFunctionDescriptor descriptor)
        => FunctionDescriptorResolver.TryResolveFunction(
            name,
            resultTypeConstraints,
            argumentTypeConstraints,
            out descriptor
        );
}