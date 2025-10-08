using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

#if NET6_0_OR_GREATER

[BuiltInDescriptor(typeof(TimeOnly?))]
public sealed partial class NullableTimeOnlyDescriptor : ITypeDescriptor
{
    object ITypeDescriptor.Parse(string value)
        => Parse(value)!;

    public IReadOnlyList<PropertyInfo> Properties { get; } = new PropertyInfo[]
    {
        (PropertyInfo)((MemberExpression)((Expression<Func<TimeOnly?, bool>>)(e => e.HasValue)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<TimeOnly?, TimeOnly>>)(e => e!.Value)).Body).Member
    };

    public bool IsArithmetic => true;

    public bool IsEnum => false;

    public bool IsValue => true;

    public bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(TimeOnly?));

    public static TimeOnly? Parse(string value)
        => string.IsNullOrEmpty(value)
            ? default(TimeOnly?)
            : TimeOnly.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);

    public string Stringify(object? value) => value switch
    {
        null => default!,
        TimeOnly dt => dt.ToString("o", CultureInfo.InvariantCulture),
        _ => throw new InvalidOperationException($"Unable to convert \"{value}\" to TimeOnly.")
    };

    public Expression CreateNotEqual(Expression self, Expression right)
        => right.Type == typeof(TimeOnly?)
            ? Expression.NotEqual(self, right)
            : right.Type == typeof(TimeOnly)
                ? Expression.NotEqual(self, Expression.Convert(right, typeof(TimeOnly?)))
                : throw new InvalidOperationException($"Cannot create NotEqual expression from TimeOnly? and {right.Type}.");

    public Expression CreateGreaterThan(Expression self, Expression right)
        => right.Type == typeof(TimeOnly?)
            ? Expression.GreaterThan(self, right)
            : right.Type == typeof(TimeOnly)
                ? Expression.GreaterThan(self, Expression.Convert(right, typeof(TimeOnly?)))
                : throw new InvalidOperationException($"Cannot create GreaterThan expression from TimeOnly? and {right.Type}.");

    public Expression CreateGreaterThanOrEqual(Expression self, Expression right)
        => right.Type == typeof(TimeOnly?)
            ? Expression.GreaterThanOrEqual(self, right)
            : right.Type == typeof(TimeOnly)
                ? Expression.GreaterThanOrEqual(self, Expression.Convert(right, typeof(TimeOnly?)))
                : throw new InvalidOperationException($"Cannot create GreaterThanOrEqual expression from TimeOnly? and {right.Type}.");

    public Expression CreateLessThan(Expression self, Expression right)
        => right.Type == typeof(TimeOnly?)
            ? Expression.LessThan(self, right)
            : right.Type == typeof(TimeOnly)
                ? Expression.LessThan(self, Expression.Convert(right, typeof(TimeOnly?)))
                : throw new InvalidOperationException($"Cannot create LessThan expression from TimeOnly? and {right.Type}.");

    public Expression CreateLessThanOrEqual(Expression self, Expression right)
        => right.Type == typeof(TimeOnly?)
            ? Expression.LessThanOrEqual(self, right)
            : right.Type == typeof(TimeOnly)
                ? Expression.LessThanOrEqual(self, Expression.Convert(right, typeof(TimeOnly?)))
                : throw new InvalidOperationException($"Cannot create LessThanOrEqual expression from TimeOnly? and {right.Type}.");

    public Expression CreateAdd(Expression self, Expression right)
        => Expression.Add(self, right);

    public Expression CreateSubtract(Expression self, Expression right)
        => Expression.Subtract(self, right);
}

#endif