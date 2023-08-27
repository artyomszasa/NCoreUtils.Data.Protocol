using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(ulong?))]
public sealed partial class NullableUInt64Descriptor : ArithmeticTypeDescriptor
{
    public override IReadOnlyList<PropertyInfo> Properties { get; } = new PropertyInfo[]
    {
        (PropertyInfo)((MemberExpression)((Expression<Func<ulong?, bool>>)(e => e.HasValue)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<ulong?, ulong>>)(e => e!.Value)).Body).Member
    };

    public override bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(ulong?));

    public override object Parse(string value)
        => ulong.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public override string? Stringify(object? value) => value is null ? default : ((ulong)value!).ToString("D", CultureInfo.InvariantCulture);
}