using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(int?))]
public sealed partial class NullableInt32Descriptor : ArithmeticTypeDescriptor
{
    public override IReadOnlyList<PropertyInfo> Properties { get; } = new PropertyInfo[]
    {
        (PropertyInfo)((MemberExpression)((Expression<Func<int?, bool>>)(e => e.HasValue)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<int?, int>>)(e => e!.Value)).Body).Member
    };

    public override bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(int?));

    public override object Parse(string value)
        => int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public override string? Stringify(object? value) => value is null ? default : ((int)value!).ToString("D", CultureInfo.InvariantCulture);
}