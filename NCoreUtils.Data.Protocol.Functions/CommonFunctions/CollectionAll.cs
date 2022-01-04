using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public sealed class CollectionAll : CollectionOperationWithLambda<CollectionAllDescriptor>
{
    private static readonly MethodInfo _genericMethodDefinition;

    static CollectionAll()
    {
        _genericMethodDefinition = ReflectionHelpers
            .GetMethod<IEnumerable<int>, Func<int, bool>, bool>(Enumerable.All)
            .GetGenericMethodDefinition();
    }

    protected override MethodInfo GenericMethodDefinition => _genericMethodDefinition;

    protected override string DefaultName => Names.Some;

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
        Justification = "Only types passed by user can appear here therefore they are preserved anyway.")]
    protected override CollectionAllDescriptor CreateDescriptorFor(Type itemType)
        => (CollectionAllDescriptor)Activator.CreateInstance(typeof(CollectionAllDescriptor<>).MakeGenericType(itemType), false)!;

    protected override bool MatchName(string name)
        => StringComparer.InvariantCultureIgnoreCase.Equals(Names.Every, name)
            || StringComparer.InvariantCultureIgnoreCase.Equals(Names.All, name);
}