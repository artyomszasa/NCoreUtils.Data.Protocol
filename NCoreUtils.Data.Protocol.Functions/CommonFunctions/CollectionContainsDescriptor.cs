using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.Internal;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public abstract class CollectionContainsDescriptor : IFunctionDescriptor
{
    private static ConcurrentDictionary<Type, CollectionContainsDescriptor> Cache { get; } = new();

    private static Func<Type, CollectionContainsDescriptor> Factory { get; } = DoCreate;

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
            Justification = "Only types passed by user can appear here therefore they are preserved anyway.")]
    private static CollectionContainsDescriptor DoCreate(Type itemType)
        => (CollectionContainsDescriptor)Activator
            .CreateInstance(typeof(CollectionContainsDescriptor<>).MakeGenericType(itemType), false)!;

    public static CollectionContainsDescriptor GetOrCreate(Type itemType)
        => Cache.GetOrAdd(itemType, Factory);

    protected abstract MethodInfo MethodContains { get; }

    public Type ResultType => typeof(bool);

    public abstract ReadOnlyConstraintedTypeList ArgumentTypes { get; }

    public string Name => Names.Includes;

    internal CollectionContainsDescriptor() { }

    public Expression CreateExpression(IReadOnlyList<Expression> arguments)
        => Expression.Call(MethodContains, arguments);
}

public sealed class CollectionContainsDescriptor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : CollectionContainsDescriptor
{
    private static readonly ReadOnlyConstraintedTypeList _argumentTypes;

    private static readonly MethodInfo _methodContains;

    static CollectionContainsDescriptor()
    {
        _argumentTypes = new ReadOnlyConstraintedTypeListBuilder
        {
            typeof(IEnumerable<T>),
            typeof(T)
        }.Build();
        _methodContains = ReflectionHelpers.GetMethod<IEnumerable<T>, T, bool>(Enumerable.Contains);
    }

    protected override MethodInfo MethodContains
        => _methodContains;

    public override ReadOnlyConstraintedTypeList ArgumentTypes
        => _argumentTypes;
}