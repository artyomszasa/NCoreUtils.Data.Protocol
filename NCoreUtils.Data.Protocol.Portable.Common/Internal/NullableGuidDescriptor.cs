using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(Guid?))]
public sealed partial class NullableGuidDescriptor : ITypeDescriptor
{
    object ITypeDescriptor.Parse(string value)
        => Parse(value)!;

    public IReadOnlyList<PropertyInfo> Properties { get; } = new PropertyInfo[]
    {
        (PropertyInfo)((MemberExpression)((Expression<Func<Guid?, bool>>)(e => e.HasValue)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<Guid?, Guid>>)(e => e!.Value)).Body).Member
    };

    public bool IsArithmetic => true;

    public bool IsEnum => false;

    public bool IsValue => true;

    public bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(Guid?));

    public static Guid? Parse(string value)
        => string.IsNullOrEmpty(value)
            ? default(Guid?)
            : Guid.Parse(value);

    public string Stringify(object? value) => value switch
    {
        null => default!,
        Guid dt => dt.ToString(),
        _ => throw new InvalidOperationException($"Unable to convert \"{value}\" to Guid.")
    };

    public Expression CreateNotEqual(Expression self, Expression right)
        => right.Type == typeof(Guid?)
            ? Expression.NotEqual(self, right)
            : right.Type == typeof(Guid)
                ? Expression.NotEqual(self, Expression.Convert(right, typeof(Guid?)))
                : throw new InvalidOperationException($"Cannot create NotEqual expression from Guid? and {right.Type}.");

    public Expression CreateGreaterThan(Expression self, Expression right)
        => right.Type == typeof(Guid?)
            ? Expression.GreaterThan(self, right)
            : right.Type == typeof(Guid)
                ? Expression.GreaterThan(self, Expression.Convert(right, typeof(Guid?)))
                : throw new InvalidOperationException($"Cannot create GreaterThan expression from Guid? and {right.Type}.");

    public Expression CreateGreaterThanOrEqual(Expression self, Expression right)
        => right.Type == typeof(Guid?)
            ? Expression.GreaterThanOrEqual(self, right)
            : right.Type == typeof(Guid)
                ? Expression.GreaterThanOrEqual(self, Expression.Convert(right, typeof(Guid?)))
                : throw new InvalidOperationException($"Cannot create GreaterThanOrEqual expression from Guid? and {right.Type}.");

    public Expression CreateLessThan(Expression self, Expression right)
        => right.Type == typeof(Guid?)
            ? Expression.LessThan(self, right)
            : right.Type == typeof(Guid)
                ? Expression.LessThan(self, Expression.Convert(right, typeof(Guid?)))
                : throw new InvalidOperationException($"Cannot create LessThan expression from Guid? and {right.Type}.");

    public Expression CreateLessThanOrEqual(Expression self, Expression right)
        => right.Type == typeof(Guid?)
            ? Expression.LessThanOrEqual(self, right)
            : right.Type == typeof(Guid)
                ? Expression.LessThanOrEqual(self, Expression.Convert(right, typeof(Guid?)))
                : throw new InvalidOperationException($"Cannot create LessThanOrEqual expression from Guid? and {right.Type}.");

    public Expression CreateAdd(Expression self, Expression right)
        => Expression.Add(self, right);

    public Expression CreateSubtract(Expression self, Expression right)
        => Expression.Subtract(self, right);
}