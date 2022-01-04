using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.Internal;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public abstract class CollectionAnyDescriptor : IFunctionDescriptor
{
    protected abstract MethodInfo MethodAny { get; }

    public Type ResultType
    {
        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        get => typeof(bool);
    }

    public abstract ReadOnlyConstraintedTypeList ArgumentTypes { get; }

    public string Name
    {
        [ExcludeFromCodeCoverage]
        get => Names.Some;
    }

    internal CollectionAnyDescriptor() { }

    public Expression CreateExpression(IReadOnlyList<Expression> arguments)
        => Expression.Call(MethodAny, arguments);
}

public sealed class CollectionAnyDescriptor<T> : CollectionAnyDescriptor
{
    private static readonly ReadOnlyConstraintedTypeList _argumentTypes;

    private static readonly MethodInfo _methodAny;

    [UnconditionalSuppressMessage("Trimming", "IL2111", Justification = "Type preservation are handled in factory methods.")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "System.Delegate is explicitely preserved.")]
    static CollectionAnyDescriptor()
    {
        _argumentTypes = new ReadOnlyConstraintedTypeListBuilder
        {
            typeof(IEnumerable<T>),
            typeof(Func<T, bool>)
        }.Build();
        _methodAny = ReflectionHelpers.GetMethod<IEnumerable<T>, Func<T, bool>, bool>(Enumerable.Any);
    }

    protected override MethodInfo MethodAny
        => _methodAny;

    public override ReadOnlyConstraintedTypeList ArgumentTypes
        => _argumentTypes;
}