using System.Collections.Generic;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol;

internal static class DefaultFunctionDescriptorResolverImplementation
{
    public static IFunctionDescriptor ResolveFunction(
        IFunctionDescriptorResolver resolver,
        IDataUtils util,
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        Func<IFunctionDescriptor> next)
        => resolver.TryResolveFunction(util, name, resultTypeConstraints, argumentTypeConstraints, out var descriptor)
            ? descriptor
            : next();
}