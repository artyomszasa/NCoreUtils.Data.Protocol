using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.Internal;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol;

public class CompositeFunctionDescriptorResolver : IAmbigousFunctionDescriptorResolver
{
    private IEnumerable<IFunctionDescriptorResolverWrapper> Resolvers { get; }

    internal CompositeFunctionDescriptorResolver(IEnumerable<IFunctionDescriptorResolverWrapper> resolvers)
        => Resolvers = resolvers;

    public bool TryResolveAllMatchingFunctions(
        IDataUtils util,
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        ICollection<IFunctionDescriptor> descriptors)
    {
        var hasMatch = false;
        foreach (var resolver in Resolvers)
        {
            if (resolver is IAmbigousFunctionDescriptorResolver ambigousResolver)
            {
                hasMatch |= ambigousResolver.TryResolveAllMatchingFunctions(util, name, resultTypeConstraints, argumentTypeConstraints, descriptors);
            }
            else if (resolver.TryResolveFunction(util, name, resultTypeConstraints, argumentTypeConstraints, out var desc))
            {
                descriptors.Add(desc);
                hasMatch = true;
            }
        }
        return hasMatch;
    }

    public bool TryResolveFunction(
        IDataUtils util,
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        [MaybeNullWhen(false)] out IFunctionDescriptor descriptor)
    {
        foreach (var resolver in Resolvers)
        {
            if (resolver.TryResolveFunction(util, name, resultTypeConstraints, argumentTypeConstraints, out var desc))
            {
                descriptor = desc;
                return true;
            }
        }
        descriptor = default;
        return false;
    }
}