using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public sealed class CollectionContainsDescriptor : IFunctionDescriptor
{
    private static ConcurrentDictionary<(IDataUtils Util, Type ElementType), CollectionContainsDescriptor> Cache { get; } = new();

    private static CollectionContainsDescriptor DoCreate(IDataUtils util, Type elementType) => new(
        methodContains: util.GetEnumerableContainsMethod(elementType),
        enumerableType: util.Ensure(util.GetEnumerableOfType(elementType)),
        elementType: util.Ensure(elementType)
    );

    public static CollectionContainsDescriptor GetOrCreate(IDataUtils util, Type elementType)
    {
        if (Cache.TryGetValue((util, elementType), out var descriptor))
        {
            return descriptor;
        }
        return Cache.GetOrAdd((util, elementType), DoCreate(util, elementType));
    }

    private MethodInfo MethodContains { get; }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type ResultType => typeof(bool);

    public ReadOnlyConstraintedTypeList ArgumentTypes { get; }

    public string Name => Names.Includes;

    internal CollectionContainsDescriptor(
        MethodInfo methodContains,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type enumerableType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type elementType)
    {
        MethodContains = methodContains;
        ArgumentTypes = new ReadOnlyConstraintedTypeListBuilder { enumerableType, elementType }.Build();
    }

    public Expression CreateExpression(IReadOnlyList<Expression> arguments)
        => Expression.Call(MethodContains, arguments);
}