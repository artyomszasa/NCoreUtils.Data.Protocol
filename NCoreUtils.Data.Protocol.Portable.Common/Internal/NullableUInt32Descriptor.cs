using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(uint?))]
public sealed partial class NullableUInt32Descriptor : ArithmeticTypeDescriptor
{
    public override IReadOnlyList<PropertyInfo> Properties { get; } = new PropertyInfo[]
    {
        (PropertyInfo)((MemberExpression)((Expression<Func<uint?, bool>>)(e => e.HasValue)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<uint?, uint>>)(e => e!.Value)).Body).Member
    };

    public override bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(uint?));

    public override object Parse(string value)
        => uint.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public override string? Stringify(object? value) => value is null ? default : ((uint)value!).ToString("D", CultureInfo.InvariantCulture);
}