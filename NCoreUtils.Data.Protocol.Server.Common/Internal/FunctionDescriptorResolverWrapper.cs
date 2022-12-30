using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.Internal;

internal sealed class FunctionDescriptorResolverWrapper : IFunctionDescriptorResolverWrapper
{
    private IFunctionDescriptorResolver Resolver { get; }

    public FunctionDescriptorResolverWrapper(IFunctionDescriptorResolver resolver)
        => Resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));

    public bool TryResolveFunction(
        IDataUtils util,
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        [MaybeNullWhen(false)] out IFunctionDescriptor descriptor)
        => Resolver.TryResolveFunction(
            util,
            name,
            resultTypeConstraints,
            argumentTypeConstraints,
            out descriptor
        );

    public void Dispose()
        => (Resolver as IDisposable)?.Dispose();
}