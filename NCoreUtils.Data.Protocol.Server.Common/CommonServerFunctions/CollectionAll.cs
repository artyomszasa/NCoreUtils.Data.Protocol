using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.TypeInference;
using Names = NCoreUtils.Data.Protocol.CommonFunctionNames;

namespace NCoreUtils.Data.Protocol.CommonServerFunctions;

public sealed class CollectionAll : IFunctionDescriptorResolver
{
    public bool TryResolveFunction(
        IDataUtils util,
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        [MaybeNullWhen(false)] out IFunctionDescriptor descriptor)
    {
        // FIXME: check arg[1] is not non-lambda
        if ((Helpers.Eqi(Names.Every, name) || Helpers.Eqi(Names.All, name))
            && resultTypeConstraints.IsCompatible<bool>(util)
            && argumentTypeConstraints.Count == 2
            && argumentTypeConstraints[0].TryGetElementType(util, out var elementType))
        {
            descriptor = new CollectionAllDescriptor(
                methodAll: util.GetEnumerableAllMethod(elementType),
                enumerableType: util.Ensure(util.GetEnumerableOfType(elementType)),
                lambdaType: util.Ensure(util.GetOrCreateLambdaType(elementType, typeof(bool)))
            );
            return true;
        }
        descriptor = default;
        return false;
    }
}