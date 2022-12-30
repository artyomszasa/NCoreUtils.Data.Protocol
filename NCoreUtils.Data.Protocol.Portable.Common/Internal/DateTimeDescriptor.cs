using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

public sealed class DateTimeDescriptor : ITypeDescriptor
{
    public sealed class Box
    {
        public readonly DateTime Value;

        public Box(DateTime value) => Value = value;

        public override string ToString() => $"{{{Value}}}";
    }

    private FieldInfo BoxValueField { get; } = (FieldInfo)((MemberExpression)((Expression<Func<Box, DateTime>>)(e => e.Value)).Body).Member;

    object ITypeDescriptor.Parse(string value)
        => Parse(value);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type Type => typeof(DateTime);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type ArrayOfType => typeof(DateTime[]);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type EnumerableOfType => typeof(IEnumerable<DateTime>);

    public IReadOnlyList<PropertyInfo> Properties { get; } = new PropertyInfo[]
    {
        (PropertyInfo)((MemberExpression)((Expression<Func<DateTime, long>>)(e => e.Ticks)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<DateTime, int>>)(e => e.Millisecond)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<DateTime, int>>)(e => e.Second)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<DateTime, int>>)(e => e.Minute)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<DateTime, int>>)(e => e.Hour)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<DateTime, int>>)(e => e.Day)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<DateTime, int>>)(e => e.Month)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<DateTime, int>>)(e => e.Year)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<DateTime, int>>)(e => e.DayOfYear)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<DateTime, DayOfWeek>>)(e => e.DayOfWeek)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<DateTime, TimeSpan>>)(e => e.TimeOfDay)).Body).Member
    };

    public bool IsArithmetic => true;

    public bool IsEnum => false;

    public bool IsValue => true;

    public object? BoxNullable(object value)
        => (DateTime?)(DateTime)value;

    public bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(DateTime));

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

    public DateTime Parse(string value)
        => DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces);

    public string Stringify(object? value) => value switch
    {
        null => throw new InvalidOperationException("Unable to convert null to DateTime."),
        DateTime dt => dt.ToString("o", CultureInfo.InvariantCulture),
        _ => throw new InvalidOperationException($"Unable to convert \"{value}\" to DateTime.")
    };

    public bool TryGetEnumFactory([MaybeNullWhen(false)] out IEnumFactory enumFactory)
    {
        enumFactory = default;
        return false;
    }

    public Expression CreateBoxedConstant(object? value)
        => Expression.Field(
            Expression.Constant(new Box((DateTime)value!)),
            BoxValueField
        );

    public Expression CreateAndAlso(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateOrElse(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateEqual(Expression self, Expression right)
        => Expression.Equal(self, right);

    public Expression CreateNotEqual(Expression self, Expression right)
        => Expression.NotEqual(self, right);

    public Expression CreateGreaterThan(Expression self, Expression right)
        => Expression.GreaterThan(self, right);

    public Expression CreateGreaterThanOrEqual(Expression self, Expression right)
        => Expression.GreaterThanOrEqual(self, right);

    public Expression CreateLessThan(Expression self, Expression right)
        => Expression.LessThan(self, right);

    public Expression CreateLessThanOrEqual(Expression self, Expression right)
        => Expression.LessThanOrEqual(self, right);

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

    public MethodInfo EnumerableAnyMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<DateTime>, Func<DateTime, bool>, bool>(Enumerable.Any);

    public MethodInfo EnumerableAllMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<DateTime>, Func<DateTime, bool>, bool>(Enumerable.All);

    public MethodInfo EnumerableContainsMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<DateTime>, DateTime, bool>(Enumerable.Contains);

    public void Accept(IDataTypeVisitor visitor)
        => visitor.Visit<DateTime>();
}