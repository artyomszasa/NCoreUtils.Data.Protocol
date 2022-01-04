using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.Internal;

public class CompositeFunctionDescriptorResolver : IAmbigousFunctionDescriptorResolver
{
    private IServiceProvider ServiceProvider { get; }

    public IReadOnlyList<IFunctionDescriptorResolverDescriptor> Resolvers { get; }

    public CompositeFunctionDescriptorResolver(IServiceProvider serviceProvider, IReadOnlyList<IFunctionDescriptorResolverDescriptor> resolvers)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Resolvers = resolvers ?? throw new ArgumentNullException(nameof(resolvers));
    }

    public bool TryResolveFunction(
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        [MaybeNullWhen(false)] out IFunctionDescriptor descriptor)
    {
        foreach (var resolverDescriptor in Resolvers)
        {
            var resolver = resolverDescriptor.GetOrCreate(ServiceProvider);
            try
            {
                if (resolver.TryResolveFunction(name, resultTypeConstraints, argumentTypeConstraints, out var desc))
                {
                    descriptor = desc;
                    return true;
                }
            }
            finally
            {
                (resolver as IDisposable)?.Dispose();
            }
        }
        descriptor = default;
        return false;
    }

    public bool TryResolveAllMatchingFunctions(
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        ICollection<IFunctionDescriptor> descriptors)
    {
        var hasMatch = false;
        foreach (var resolverDescriptor in Resolvers)
        {
            var resolver = resolverDescriptor.GetOrCreate(ServiceProvider);
            try
            {
                if (resolver is IAmbigousFunctionDescriptorResolver ambigousResolver)
                {
                    hasMatch |= ambigousResolver.TryResolveAllMatchingFunctions(name, resultTypeConstraints, argumentTypeConstraints, descriptors);
                }
                else if (resolver.TryResolveFunction(name, resultTypeConstraints, argumentTypeConstraints, out var desc))
                {
                    descriptors.Add(desc);
                    hasMatch = true;
                }
            }
            finally
            {
                (resolver as IDisposable)?.Dispose();
            }
        }
        return hasMatch;
    }
}