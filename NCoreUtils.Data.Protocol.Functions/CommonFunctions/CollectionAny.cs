using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NCoreUtils.Data.Protocol.Internal;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public sealed class CollectionAny : CollectionOperationWithLambda
{
    private static readonly MethodInfo _genericMethodDefinition;

    static CollectionAny()
    {
        _genericMethodDefinition = ReflectionHelpers
            .GetMethod<IEnumerable<int>, Func<int, bool>, bool>(Enumerable.Any)
            .GetGenericMethodDefinition();
    }

    protected override MethodInfo GenericMethodDefinition => _genericMethodDefinition;

    protected override string DefaultName => Names.Some;

    public CollectionAny() : base(CollectionAnyUid) { }

    protected override IFunctionDescriptor CreateDescriptorFor(IDataUtils util, Type itemType)
        => new CollectionAnyDescriptor(util.GetEnumerableAnyMethod(itemType));

    protected override bool MatchName(string name)
        => StringComparer.InvariantCultureIgnoreCase.Equals(Names.Some, name)
            || StringComparer.InvariantCultureIgnoreCase.Equals(Names.Any, name);
}