using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

#if NET6_0_OR_GREATER

[BuiltInDescriptor(typeof(DateOnly))]
public sealed partial class DateOnlyDescriptor : ITypeDescriptor
{
    object ITypeDescriptor.Parse(string value)
        => Parse(value);

    public IReadOnlyList<PropertyInfo> Properties { get; } = new PropertyInfo[]
    {
        (PropertyInfo)((MemberExpression)((Expression<Func<DateOnly, int>>)(e => e.Day)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<DateOnly, int>>)(e => e.Month)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<DateOnly, int>>)(e => e.Year)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<DateOnly, int>>)(e => e.DayOfYear)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<DateOnly, DayOfWeek>>)(e => e.DayOfWeek)).Body).Member
    };

    public bool IsArithmetic => true;

    public bool IsEnum => false;

    public bool IsValue => true;

    public bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(DateOnly)) || baseType.Equals(typeof(DateOnly?));

    public static DateOnly Parse(string value)
        => DateOnly.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);

    public string Stringify(object? value) => value switch
    {
        null => throw new InvalidOperationException("Unable to convert null to DateOnly."),
        DateOnly dt => dt.ToString("o", CultureInfo.InvariantCulture),
        _ => throw new InvalidOperationException($"Unable to convert \"{value}\" to DateOnly.")
    };

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
}

#endif