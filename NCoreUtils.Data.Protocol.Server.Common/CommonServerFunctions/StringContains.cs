using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.TypeInference;
using Names = NCoreUtils.Data.Protocol.CommonFunctionNames;

namespace NCoreUtils.Data.Protocol.CommonServerFunctions;

public sealed class StringContains : IFunctionDescriptorResolver
{
    public bool TryResolveFunction(
        IDataUtils util,
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        [MaybeNullWhen(false)] out IFunctionDescriptor descriptor)
    {
        if (StringComparer.InvariantCultureIgnoreCase.Equals(name, Names.Contains)
            && resultTypeConstraints.IsCompatible<bool>(util)
            && argumentTypeConstraints.Count == 2
            && argumentTypeConstraints[0].IsCompatible<string>(util)
            && argumentTypeConstraints[1].IsCompatible<string>(util))
        {
            descriptor = StringContainsDescriptor.Singleton;
            return true;
        }
        descriptor = default;
        return false;
    }
}