using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

#if NET6_0_OR_GREATER

[BuiltInDescriptor(typeof(DateOnly?))]
public sealed partial class NullableDateOnlyDescriptor : ITypeDescriptor
{
    object ITypeDescriptor.Parse(string value)
        => Parse(value)!;

    public IReadOnlyList<PropertyInfo> Properties { get; } = new PropertyInfo[]
    {
        (PropertyInfo)((MemberExpression)((Expression<Func<DateOnly?, bool>>)(e => e.HasValue)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<DateOnly?, DateOnly>>)(e => e!.Value)).Body).Member
    };

    public bool IsArithmetic => true;

    public bool IsEnum => false;

    public bool IsValue => true;

    public bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(DateOnly?));

    public static DateOnly? Parse(string value)
        => string.IsNullOrEmpty(value)
            ? default(DateOnly?)
            : DateOnly.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);

    public string Stringify(object? value) => value switch
    {
        null => default!,
        DateOnly dt => dt.ToString("o", CultureInfo.InvariantCulture),
        _ => throw new InvalidOperationException($"Unable to convert \"{value}\" to DateOnly.")
    };

    public Expression CreateNotEqual(Expression self, Expression right)
        => right.Type == typeof(DateOnly?)
            ? Expression.NotEqual(self, right)
            : right.Type == typeof(DateOnly)
                ? Expression.NotEqual(self, Expression.Convert(right, typeof(DateOnly?)))
                : throw new InvalidOperationException($"Cannot create NotEqual expression from DateOnly? and {right.Type}.");

    public Expression CreateGreaterThan(Expression self, Expression right)
        => right.Type == typeof(DateOnly?)
            ? Expression.GreaterThan(self, right)
            : right.Type == typeof(DateOnly)
                ? Expression.GreaterThan(self, Expression.Convert(right, typeof(DateOnly?)))
                : throw new InvalidOperationException($"Cannot create GreaterThan expression from DateOnly? and {right.Type}.");

    public Expression CreateGreaterThanOrEqual(Expression self, Expression right)
        => right.Type == typeof(DateOnly?)
            ? Expression.GreaterThanOrEqual(self, right)
            : right.Type == typeof(DateOnly)
                ? Expression.GreaterThanOrEqual(self, Expression.Convert(right, typeof(DateOnly?)))
                : throw new InvalidOperationException($"Cannot create GreaterThanOrEqual expression from DateOnly? and {right.Type}.");

    public Expression CreateLessThan(Expression self, Expression right)
        => right.Type == typeof(DateOnly?)
            ? Expression.LessThan(self, right)
            : right.Type == typeof(DateOnly)
                ? Expression.LessThan(self, Expression.Convert(right, typeof(DateOnly?)))
                : throw new InvalidOperationException($"Cannot create LessThan expression from DateOnly? and {right.Type}.");

    public Expression CreateLessThanOrEqual(Expression self, Expression right)
        => right.Type == typeof(DateOnly?)
            ? Expression.LessThanOrEqual(self, right)
            : right.Type == typeof(DateOnly)
                ? Expression.LessThanOrEqual(self, Expression.Convert(right, typeof(DateOnly?)))
                : throw new InvalidOperationException($"Cannot create LessThanOrEqual expression from DateOnly? and {right.Type}.");

    public Expression CreateAdd(Expression self, Expression right)
        => Expression.Add(self, right);

    public Expression CreateSubtract(Expression self, Expression right)
        => Expression.Subtract(self, right);
}

#endif