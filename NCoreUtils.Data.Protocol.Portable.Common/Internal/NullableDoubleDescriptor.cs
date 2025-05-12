using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(double?))]
public sealed partial class NullableDoubleDescriptor : ArithmeticTypeDescriptor
{
    public static double? ParseDouble(string value)
        => string.IsNullOrEmpty(value)
            ? default(double?)
            : double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);

    public override IReadOnlyList<PropertyInfo> Properties { get; } = new PropertyInfo[]
    {
        (PropertyInfo)((MemberExpression)((Expression<Func<double?, bool>>)(e => e.HasValue)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<double?, double>>)(e => e!.Value)).Body).Member
    };

    public override bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(double?));

    public override object Parse(string value)
        => ParseDouble(value)!;

    public override string? Stringify(object? value) => value is null ? default : ((double)value!).ToString("G", CultureInfo.InvariantCulture);
}