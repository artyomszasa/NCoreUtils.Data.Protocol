using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

 #if NET6_0_OR_GREATER

[BuiltInDescriptor(typeof(TimeOnly))]
public sealed partial class TimeOnlyDescriptor : ITypeDescriptor
{
    object ITypeDescriptor.Parse(string value)
        => Parse(value);

    public IReadOnlyList<PropertyInfo> Properties { get; } = new PropertyInfo[]
    {
        (PropertyInfo)((MemberExpression)((Expression<Func<TimeOnly, int>>)(e => e.Hour)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<TimeOnly, int>>)(e => e.Minute)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<TimeOnly, int>>)(e => e.Second)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<TimeOnly, int>>)(e => e.Millisecond)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<TimeOnly, long>>)(e => e.Ticks)).Body).Member
    };

    public bool IsArithmetic => true;

    public bool IsEnum => false;

    public bool IsValue => true;

    public bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(TimeOnly)) || baseType.Equals(typeof(TimeOnly?));

    public static TimeOnly Parse(string value)
        => TimeOnly.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);

    public string Stringify(object? value) => value switch
    {
        null => throw new InvalidOperationException("Unable to convert null to TimeOnly."),
        TimeOnly dt => dt.ToString("o", CultureInfo.InvariantCulture),
        _ => throw new InvalidOperationException($"Unable to convert \"{value}\" to TimeOnly.")
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