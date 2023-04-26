using System;
using System.Globalization;
using System.Linq.Expressions;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(DateTimeOffset?))]
public sealed partial class NullableDateTimeOffsetDescriptor : ITypeDescriptor
{
    object ITypeDescriptor.Parse(string value)
        => Parse(value)!;

    public bool IsArithmetic => true;

    public bool IsEnum => false;

    public bool IsValue => true;

    public bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(DateTimeOffset?));

    public static DateTimeOffset? Parse(string value)
        => string.IsNullOrEmpty(value)
            ? default(DateTimeOffset?)
            : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);

    public string Stringify(object? value) => value switch
    {
        null => default!,
        DateTimeOffset dt => dt.ToString("o", CultureInfo.InvariantCulture),
        _ => throw new InvalidOperationException($"Unable to convert \"{value}\" to DateTimeOffset.")
    };

    public Expression CreateNotEqual(Expression self, Expression right)
        => right.Type == typeof(DateTimeOffset?)
            ? Expression.NotEqual(self, right)
            : right.Type == typeof(DateTimeOffset)
                ? Expression.NotEqual(self, Expression.Convert(right, typeof(DateTimeOffset?)))
                : throw new InvalidOperationException($"Cannot create NotEqual expression from DateTimeOffset? and {right.Type}.");

    public Expression CreateGreaterThan(Expression self, Expression right)
        => right.Type == typeof(DateTimeOffset?)
            ? Expression.GreaterThan(self, right)
            : right.Type == typeof(DateTimeOffset)
                ? Expression.GreaterThan(self, Expression.Convert(right, typeof(DateTimeOffset?)))
                : throw new InvalidOperationException($"Cannot create GreaterThan expression from DateTimeOffset? and {right.Type}.");

    public Expression CreateGreaterThanOrEqual(Expression self, Expression right)
        => right.Type == typeof(DateTimeOffset?)
            ? Expression.GreaterThanOrEqual(self, right)
            : right.Type == typeof(DateTimeOffset)
                ? Expression.GreaterThanOrEqual(self, Expression.Convert(right, typeof(DateTimeOffset?)))
                : throw new InvalidOperationException($"Cannot create GreaterThanOrEqual expression from DateTimeOffset? and {right.Type}.");

    public Expression CreateLessThan(Expression self, Expression right)
        => right.Type == typeof(DateTimeOffset?)
            ? Expression.LessThan(self, right)
            : right.Type == typeof(DateTimeOffset)
                ? Expression.LessThan(self, Expression.Convert(right, typeof(DateTimeOffset?)))
                : throw new InvalidOperationException($"Cannot create LessThan expression from DateTimeOffset? and {right.Type}.");

    public Expression CreateLessThanOrEqual(Expression self, Expression right)
        => right.Type == typeof(DateTimeOffset?)
            ? Expression.LessThanOrEqual(self, right)
            : right.Type == typeof(DateTimeOffset)
                ? Expression.LessThanOrEqual(self, Expression.Convert(right, typeof(DateTimeOffset?)))
                : throw new InvalidOperationException($"Cannot create LessThanOrEqual expression from DateTimeOffset? and {right.Type}.");

    public Expression CreateAdd(Expression self, Expression right)
        => Expression.Add(self, right);

    public Expression CreateSubtract(Expression self, Expression right)
        => Expression.Subtract(self, right);
}