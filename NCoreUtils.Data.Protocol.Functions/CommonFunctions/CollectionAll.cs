using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NCoreUtils.Data.Protocol.Internal;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public sealed class CollectionAll : CollectionOperationWithLambda
{
    private static readonly MethodInfo _genericMethodDefinition;

    static CollectionAll()
    {
        _genericMethodDefinition = ReflectionHelpers
            .GetMethod<IEnumerable<int>, Func<int, bool>, bool>(Enumerable.All)
            .GetGenericMethodDefinition();
    }

    protected override MethodInfo GenericMethodDefinition => _genericMethodDefinition;

    protected override string DefaultName => Names.Every;

    public CollectionAll() : base(CollectionAllUid) { }

    protected override IFunctionDescriptor CreateDescriptorFor(IDataUtils util, Type itemType)
        => new CollectionAllDescriptor(util.GetEnumerableAllMethod(itemType));

    protected override bool MatchName(string name)
        => StringComparer.InvariantCultureIgnoreCase.Equals(Names.Every, name)
            || StringComparer.InvariantCultureIgnoreCase.Equals(Names.All, name);
}