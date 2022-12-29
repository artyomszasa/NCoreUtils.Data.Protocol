using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

public sealed class NullableDateTimeOffsetDescriptor : ITypeDescriptor
{
    public sealed class Box
    {
        public readonly DateTimeOffset? Value;

        public Box(DateTimeOffset? value) => Value = value;
    }

    private FieldInfo BoxValueField { get; } = (FieldInfo)((MemberExpression)((Expression<Func<Box, DateTimeOffset?>>)(e => e.Value)).Body).Member;

    object ITypeDescriptor.Parse(string value)
        => Parse(value)!;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type Type => typeof(DateTimeOffset?);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type ArrayOfType => typeof(DateTimeOffset?[]);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type EnumerableOfType => typeof(IEnumerable<DateTimeOffset?>);

    public IReadOnlyList<PropertyInfo> Properties { get; } = new PropertyInfo[]
    {
        (PropertyInfo)((MemberExpression)((Expression<Func<DateTimeOffset?, DateTimeOffset>>)(e => e!.Value)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<DateTimeOffset?, bool>>)(e => e.HasValue)).Body).Member
    };

    public bool IsArithmetic => true;

    public bool IsEnum => false;

    public bool IsValue => true;

    public object? BoxNullable(object value)
        => throw new InvalidOperationException("Unable to create nullable from nullable.");

    public bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(DateTimeOffset?));

    public bool IsEnumerable([MaybeNullWhen(false)] out Type elementType)
    {
        elementType = default;
        return false;
    }

    public bool IsArray([MaybeNullWhen(false)] out Type elementType)
    {
        elementType = default;
        return false;
    }

    public bool IsLambda([MaybeNullWhen(false)] out Type argType, [MaybeNullWhen(false)] out Type resType)
    {
        argType = default;
        resType = default;
        return false;
    }

    public bool IsMaybe([MaybeNullWhen(false)] out Type elementType)
    {
        elementType = default;
        return false;
    }

    public bool IsNullable([MaybeNullWhen(false)] out Type elementType)
    {
        elementType = default;
        return false;
    }

    public DateTimeOffset? Parse(string value)
        => string.IsNullOrEmpty(value)
            ? default(DateTimeOffset?)
            : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);

    public string Stringify(object? value) => value switch
    {
        null => default!,
        DateTimeOffset dt => dt.ToString("o", CultureInfo.InvariantCulture),
        _ => throw new InvalidOperationException($"Unable to convert \"{value}\" to DateTimeOffset.")
    };

    public bool TryGetEnumFactory([MaybeNullWhen(false)] out IEnumFactory enumFactory)
    {
        enumFactory = default;
        return false;
    }

    public Expression CreateBoxedConstant(object? value)
        => Expression.Field(
            Expression.Constant(new Box((DateTimeOffset?)value)),
            BoxValueField
        );

    public Expression CreateAndAlso(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateOrElse(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateEqual(Expression self, Expression right)
        => right.Type == typeof(DateTimeOffset?)
            ? Expression.Equal(self, right)
            : right.Type == typeof(DateTimeOffset)
                ? Expression.Equal(self, Expression.Convert(right, typeof(DateTimeOffset?)))
                : throw new InvalidOperationException($"Cannot create Equal expression from DateTmeOffset? and {right.Type}.");

    public Expression CreateNotEqual(Expression self, Expression right)
        => right.Type == typeof(DateTimeOffset?)
            ? Expression.NotEqual(self, right)
            : right.Type == typeof(DateTimeOffset)
                ? Expression.NotEqual(self, Expression.Convert(right, typeof(DateTimeOffset?)))
                : throw new InvalidOperationException($"Cannot create NotEqual expression from DateTmeOffset? and {right.Type}.");

    public Expression CreateGreaterThan(Expression self, Expression right)
        => right.Type == typeof(DateTimeOffset?)
            ? Expression.GreaterThan(self, right)
            : right.Type == typeof(DateTimeOffset)
                ? Expression.GreaterThan(self, Expression.Convert(right, typeof(DateTimeOffset?)))
                : throw new InvalidOperationException($"Cannot create GreaterThan expression from DateTmeOffset? and {right.Type}.");

    public Expression CreateGreaterThanOrEqual(Expression self, Expression right)
        => right.Type == typeof(DateTimeOffset?)
            ? Expression.GreaterThanOrEqual(self, right)
            : right.Type == typeof(DateTimeOffset)
                ? Expression.GreaterThanOrEqual(self, Expression.Convert(right, typeof(DateTimeOffset?)))
                : throw new InvalidOperationException($"Cannot create GreaterThanOrEqual expression from DateTmeOffset? and {right.Type}.");

    public Expression CreateLessThan(Expression self, Expression right)
        => right.Type == typeof(DateTimeOffset?)
            ? Expression.LessThan(self, right)
            : right.Type == typeof(DateTimeOffset)
                ? Expression.LessThan(self, Expression.Convert(right, typeof(DateTimeOffset?)))
                : throw new InvalidOperationException($"Cannot create LessThan expression from DateTmeOffset? and {right.Type}.");

    public Expression CreateLessThanOrEqual(Expression self, Expression right)
        => right.Type == typeof(DateTimeOffset?)
            ? Expression.LessThanOrEqual(self, right)
            : right.Type == typeof(DateTimeOffset)
                ? Expression.LessThanOrEqual(self, Expression.Convert(right, typeof(DateTimeOffset?)))
                : throw new InvalidOperationException($"Cannot create LessThanOrEqual expression from DateTmeOffset? and {right.Type}.");

    public Expression CreateAdd(Expression self, Expression right)
        => Expression.Add(self, right);

    public Expression CreateSubtract(Expression self, Expression right)
        => Expression.Subtract(self, right);

    public Expression CreateMultiply(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateDivide(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateModulo(Expression self, Expression right)
        => throw new NotSupportedException();

    public MethodInfo EnumerableAnyMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<DateTimeOffset?>, Func<DateTimeOffset?, bool>, bool>(Enumerable.Any);

    public MethodInfo EnumerableAllMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<DateTimeOffset?>, Func<DateTimeOffset?, bool>, bool>(Enumerable.All);
}