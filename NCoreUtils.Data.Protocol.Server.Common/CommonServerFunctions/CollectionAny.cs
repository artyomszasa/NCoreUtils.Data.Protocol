using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.TypeInference;
using Names = NCoreUtils.Data.Protocol.CommonFunctionNames;

namespace NCoreUtils.Data.Protocol.CommonServerFunctions;

public sealed class CollectionAny : IFunctionDescriptorResolver
{
    public bool TryResolveFunction(
        IDataUtils util,
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        [MaybeNullWhen(false)] out IFunctionDescriptor descriptor)
    {
        // FIXME: check arg[1] is not non-lambda
        if ((Helpers.Eqi(Names.Some, name) || Helpers.Eqi(Names.Any, name))
            && resultTypeConstraints.IsCompatible<bool>(util)
            && argumentTypeConstraints.Count == 2
            && argumentTypeConstraints[0].TryGetElementType(util, out var elementType))
        {
            descriptor = new CollectionAnyDescriptor(
                methodAny: util.GetEnumerableAnyMethod(elementType),
                enumerableType: util.Ensure(util.GetEnumerableOfType(elementType)),
                lambdaType: util.Ensure(util.GetOrCreateLambdaType(elementType, typeof(bool)))
            );
            return true;
        }
        descriptor = default;
        return false;
    }
}