using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(float?))]
public sealed partial class NullableSingleDescriptor : ArithmeticTypeDescriptor
{
    public static float? ParseSingle(string value)
        => string.IsNullOrEmpty(value)
            ? default(float?)
            : float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);

    public override IReadOnlyList<PropertyInfo> Properties { get; } = new PropertyInfo[]
    {
        (PropertyInfo)((MemberExpression)((Expression<Func<float?, bool>>)(e => e.HasValue)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<float?, float>>)(e => e!.Value)).Body).Member
    };

    public override bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(float?));

    public override object Parse(string value)
        => ParseSingle(value)!;

    public override string? Stringify(object? value) => value is null ? default : ((float)value!).ToString("G", CultureInfo.InvariantCulture);
}