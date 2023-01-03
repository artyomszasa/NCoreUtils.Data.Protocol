using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.TypeInference;
using Names = NCoreUtils.Data.Protocol.CommonFunctionNames;

namespace NCoreUtils.Data.Protocol.CommonServerFunctions;

public sealed class DateTimeOffsetFun : IFunctionDescriptorResolver
{
    public bool TryResolveFunction(
        IDataUtils util,
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        [MaybeNullWhen(false)] out IFunctionDescriptor descriptor)
    {
        if (StringComparer.InvariantCultureIgnoreCase.Equals(name, Names.DateTimeOffset)
            && argumentTypeConstraints.Count == 1
            && argumentTypeConstraints[0].IsCompatible<long>(util)
            && resultTypeConstraints.IsCompatible<DateTimeOffset>(util))
        {
            descriptor = DateTimeOffsetDescriptor.Singleton;
            return true;
        }
        descriptor = default;
        return false;
    }
}