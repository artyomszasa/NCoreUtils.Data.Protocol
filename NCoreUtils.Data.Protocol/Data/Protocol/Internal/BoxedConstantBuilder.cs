using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NCoreUtils.Data.Protocol.Internal;

public abstract class BoxedConstantBuilder
{
    private static ConcurrentDictionary<Type, BoxedConstantBuilder> Cache { get; } = new();

    private static Func<Type, BoxedConstantBuilder> Factory { get; } = Create;

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
            Justification = "Usually constant expression are created for primitive types, otherwise client must preserve possible types.")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2055",
            Justification = "Usually constant expression are created for primitive types, otherwise client must preserve possible types.")]
    private static BoxedConstantBuilder Create(Type type)
        => (BoxedConstantBuilder)Activator.CreateInstance(typeof(BoxedConstantBuilder<>).MakeGenericType(type), true)!;

    public static Expression BuildExpression(object value, Type type)
        => Cache.GetOrAdd(type, Factory)
            .BuildExpression(value);

    internal BoxedConstantBuilder() { }

    protected abstract Expression BuildExpression(object value);
}

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public sealed class BoxedConstantBuilder<T> : BoxedConstantBuilder
{
    private static PropertyInfo ValueProperty { get; }

    static BoxedConstantBuilder()
    {
        Expression<Func<ValueBox<T>, T>> expression = e => e.Value;
        ValueProperty = (PropertyInfo)((MemberExpression)expression.Body).Member;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Expression BuildExpression(T value)
        => Expression.Property(
            Expression.Constant(new ValueBox<T>(value)),
            ValueProperty
        );

    protected override Expression BuildExpression(object value)
        => BuildExpression((T)value);
}