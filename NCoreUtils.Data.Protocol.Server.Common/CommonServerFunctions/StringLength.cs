using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.TypeInference;
using Names = NCoreUtils.Data.Protocol.CommonFunctionNames;

namespace NCoreUtils.Data.Protocol.CommonServerFunctions;

public sealed class StringLength : IFunctionDescriptorResolver
{
    public bool TryResolveFunction(
        IDataUtils util,
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        [MaybeNullWhen(false)] out IFunctionDescriptor descriptor)
    {
        if (StringComparer.InvariantCultureIgnoreCase.Equals(name, Names.Length)
            && resultTypeConstraints.IsCompatible<int>(util)
            && argumentTypeConstraints.Count == 1
            && argumentTypeConstraints[0].IsCompatible<string>(util))
        {
            descriptor = StringLengthDescriptor.Singleton;
            return true;
        }
        descriptor = default;
        return false;
    }

#if NETFRAMEWORK
    public IFunctionDescriptor ResolveFunction(
        IDataUtils util,
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        Func<IFunctionDescriptor> next)
        => DefaultFunctionDescriptorResolverImplementation
            .ResolveFunction(this, util, name, resultTypeConstraints, argumentTypeConstraints, next);
#endif
}