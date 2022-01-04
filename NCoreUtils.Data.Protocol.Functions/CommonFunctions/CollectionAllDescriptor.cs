using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.Internal;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public abstract class CollectionAllDescriptor : IFunctionDescriptor
{
    protected abstract MethodInfo MethodAll { get; }

    public Type ResultType
    {
        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        get => typeof(bool);
    }

    public abstract ReadOnlyConstraintedTypeList ArgumentTypes { get; }

    public string Name
    {
        [ExcludeFromCodeCoverage]
        get => Names.Every;
    }

    internal CollectionAllDescriptor() { }

    public Expression CreateExpression(IReadOnlyList<Expression> arguments)
        => Expression.Call(MethodAll, arguments);
}

public sealed class CollectionAllDescriptor<T> : CollectionAllDescriptor
{
    private static readonly ReadOnlyConstraintedTypeList _argumentTypes;

    private static readonly MethodInfo _methodAll;

    [UnconditionalSuppressMessage("Trimming", "IL2111", Justification = "Type preservation are handled in factory methods.")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "System.Delegate is explicitely preserved.")]
    static CollectionAllDescriptor()
    {
        _argumentTypes = new ReadOnlyConstraintedTypeListBuilder
        {
            typeof(IEnumerable<T>),
            typeof(Func<T, bool>)
        }.Build();
        _methodAll = ReflectionHelpers.GetMethod<IEnumerable<T>, Func<T, bool>, bool>(Enumerable.All);
    }

    protected override MethodInfo MethodAll
        => _methodAll;

    public override ReadOnlyConstraintedTypeList ArgumentTypes
        => _argumentTypes;
}