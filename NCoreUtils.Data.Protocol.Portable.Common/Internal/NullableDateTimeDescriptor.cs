using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(DateTime?))]
public sealed partial class NullableDateTimeDescriptor : ITypeDescriptor
{
    object ITypeDescriptor.Parse(string value)
        => Parse(value)!;

    public IReadOnlyList<PropertyInfo> Properties { get; } = new PropertyInfo[]
    {
        (PropertyInfo)((MemberExpression)((Expression<Func<DateTime?, bool>>)(e => e.HasValue)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<DateTime?, DateTime>>)(e => e!.Value)).Body).Member
    };

    public bool IsArithmetic => true;

    public bool IsEnum => false;

    public bool IsValue => true;

    public bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(DateTime?));

    public static DateTime? Parse(string value)
        => string.IsNullOrEmpty(value)
            ? default(DateTime?)
            : DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);

    public string Stringify(object? value) => value switch
    {
        null => default!,
        DateTime dt => dt.ToString("o", CultureInfo.InvariantCulture),
        _ => throw new InvalidOperationException($"Unable to convert \"{value}\" to DateTime.")
    };

    public Expression CreateNotEqual(Expression self, Expression right)
        => right.Type == typeof(DateTime?)
            ? Expression.NotEqual(self, right)
            : right.Type == typeof(DateTime)
                ? Expression.NotEqual(self, Expression.Convert(right, typeof(DateTime?)))
                : throw new InvalidOperationException($"Cannot create NotEqual expression from DateTime? and {right.Type}.");

    public Expression CreateGreaterThan(Expression self, Expression right)
        => right.Type == typeof(DateTime?)
            ? Expression.GreaterThan(self, right)
            : right.Type == typeof(DateTime)
                ? Expression.GreaterThan(self, Expression.Convert(right, typeof(DateTime?)))
                : throw new InvalidOperationException($"Cannot create GreaterThan expression from DateTime? and {right.Type}.");

    public Expression CreateGreaterThanOrEqual(Expression self, Expression right)
        => right.Type == typeof(DateTime?)
            ? Expression.GreaterThanOrEqual(self, right)
            : right.Type == typeof(DateTime)
                ? Expression.GreaterThanOrEqual(self, Expression.Convert(right, typeof(DateTime?)))
                : throw new InvalidOperationException($"Cannot create GreaterThanOrEqual expression from DateTime? and {right.Type}.");

    public Expression CreateLessThan(Expression self, Expression right)
        => right.Type == typeof(DateTime?)
            ? Expression.LessThan(self, right)
            : right.Type == typeof(DateTime)
                ? Expression.LessThan(self, Expression.Convert(right, typeof(DateTime?)))
                : throw new InvalidOperationException($"Cannot create LessThan expression from DateTime? and {right.Type}.");

    public Expression CreateLessThanOrEqual(Expression self, Expression right)
        => right.Type == typeof(DateTime?)
            ? Expression.LessThanOrEqual(self, right)
            : right.Type == typeof(DateTime)
                ? Expression.LessThanOrEqual(self, Expression.Convert(right, typeof(DateTime?)))
                : throw new InvalidOperationException($"Cannot create LessThanOrEqual expression from DateTime? and {right.Type}.");

    public Expression CreateAdd(Expression self, Expression right)
        => Expression.Add(self, right);

    public Expression CreateSubtract(Expression self, Expression right)
        => Expression.Subtract(self, right);
}