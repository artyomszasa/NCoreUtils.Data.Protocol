using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NCoreUtils.Data.Protocol.Internal;

[RequiresDynamicCode("Should only be used in full-reflection/manually trimmed context.")]
[RequiresUnreferencedCode("Should only be used in full-reflection/manually trimmed context.")]
public abstract class BoxedConstantBuilder
{
    private static ConcurrentDictionary<Type, BoxedConstantBuilder> Cache { get; }

    private static Func<Type, BoxedConstantBuilder> Factory { get; }

    static BoxedConstantBuilder()
    {
        Cache = new();
        Factory = Create;
    }

    private static BoxedConstantBuilder Create(Type type)
        => (BoxedConstantBuilder)Activator.CreateInstance(typeof(BoxedConstantBuilder<>).MakeGenericType(type), true)!;

    public static Expression BuildExpression(object? value, Type type)
        => Cache.GetOrAdd(type, Factory)
            .BuildExpression(value);

    internal BoxedConstantBuilder() { }

    protected abstract Expression BuildExpression(object? value);
}

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[UnconditionalSuppressMessage("Trimming", "IL2109", Justification = "Only required for base type because instances of this type are created using reflection.")]
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

    protected override Expression BuildExpression(object? value)
        => BuildExpression((T)value!);
}