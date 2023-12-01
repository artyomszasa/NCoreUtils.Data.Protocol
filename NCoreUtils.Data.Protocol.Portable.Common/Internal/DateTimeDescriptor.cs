using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(DateTime))]
public sealed partial class DateTimeDescriptor : ITypeDescriptor
{
    object ITypeDescriptor.Parse(string value)
        => Parse(value);

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

    public bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(DateTime)) || baseType.Equals(typeof(DateTime?));

    public static DateTime Parse(string value)
        => DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces);

    public string Stringify(object? value) => value switch
    {
        null => throw new InvalidOperationException("Unable to convert null to DateTime."),
        DateTime dt => dt.ToString("o", CultureInfo.InvariantCulture),
        _ => throw new InvalidOperationException($"Unable to convert \"{value}\" to DateTime.")
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