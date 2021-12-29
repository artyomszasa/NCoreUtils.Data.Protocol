using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public sealed class CollectionAny : CollectionOperationWithLambda<CollectionAnyDescriptor>
{
    private static readonly MethodInfo _genericMethodDefinition = ReflectionHelpers
        .GetMethod<IEnumerable<int>, Func<int, bool>, bool>(Enumerable.Any)
        .GetGenericMethodDefinition();

    protected override MethodInfo GenericMethodDefinition => _genericMethodDefinition;

    protected override string DefaultName => Names.Some;

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
        Justification = "Only types passed by user can appear here therefore they are preserved anyway.")]
    protected override CollectionAnyDescriptor CreateDescriptorFor(Type itemType)
        => (CollectionAnyDescriptor)Activator.CreateInstance(typeof(CollectionAnyDescriptor<>).MakeGenericType(itemType), false)!;

    protected override bool MatchName(string name)
        => StringComparer.InvariantCultureIgnoreCase.Equals(Names.Some, name)
            || StringComparer.InvariantCultureIgnoreCase.Equals(Names.Any, name);
}