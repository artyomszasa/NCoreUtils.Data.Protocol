using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.TypeInference;
using Names = NCoreUtils.Data.Protocol.CommonFunctionNames;

namespace NCoreUtils.Data.Protocol.CommonServerFunctions;

public sealed class CollectionContains : IFunctionDescriptorResolver
{
    public bool TryResolveFunction(
        IDataUtils util,
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        [MaybeNullWhen(false)] out IFunctionDescriptor descriptor)
    {
        if ((Helpers.Eqi(Names.Contains, name) || Helpers.Eqi(Names.Includes, name)) && argumentTypeConstraints.Count == 2)
        {
            var elementType = argumentTypeConstraints[1].TryGetExactType(out var exactType) ? (Type)exactType : default;
            if (elementType is not null || argumentTypeConstraints[0].TryGetElementType(util, out elementType))
            {
                descriptor = new CollectionContainsDescriptor(
                    methodContains: util.GetEnumerableContainsMethod(elementType),
                    enumerableType: util.Ensure(util.GetEnumerableOfType(elementType)),
                    elementType: util.Ensure(elementType)
                );
                return true;
            }
        }
        descriptor = default;
        return false;
    }
}