using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(long?))]
public sealed partial class NullableInt64Descriptor : ArithmeticTypeDescriptor
{
    public static long? ParseInt64(string value)
        => string.IsNullOrEmpty(value)
            ? default(long?)
            : long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public override IReadOnlyList<PropertyInfo> Properties { get; } = new PropertyInfo[]
    {
        (PropertyInfo)((MemberExpression)((Expression<Func<long?, bool>>)(e => e.HasValue)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<long?, long>>)(e => e!.Value)).Body).Member
    };

    public override bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(long?));

    public override object Parse(string value)
        => ParseInt64(value)!;

    public override string? Stringify(object? value) => value is null ? default : ((long)value!).ToString("D", CultureInfo.InvariantCulture);
}