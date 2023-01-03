using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.TypeInference;
using Names = NCoreUtils.Data.Protocol.CommonFunctionNames;

namespace NCoreUtils.Data.Protocol.CommonServerFunctions;

public class ArrayOf : IFunctionDescriptorResolver
{
    private static bool TryGetArgumentType(
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        [MaybeNullWhen(false)] out Type elementType)
    {
        foreach (var argTypeConstraints in argumentTypeConstraints)
        {
            if (argTypeConstraints.TryGetExactType(out var type))
            {
                elementType = type;
                return true;
            }
        }
        elementType = default;
        return false;
    }

    public bool TryResolveFunction(
        IDataUtils util,
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        [MaybeNullWhen(false)] out IFunctionDescriptor descriptor)
    {
        if (StringComparer.InvariantCultureIgnoreCase.Equals(Names.Array, name))
        {
            if (resultTypeConstraints.TryGetElementType(util, out var elementType)
                || TryGetArgumentType(argumentTypeConstraints, out elementType))
            {
                descriptor = new ArrayOfDescriptor(
                    util.Ensure(util.GetArrayOfType(elementType)),
                    util.Ensure(elementType),
                    argumentTypeConstraints.Count
                );
                return true;
            }
        }
        descriptor = default;
        return false;
    }
}