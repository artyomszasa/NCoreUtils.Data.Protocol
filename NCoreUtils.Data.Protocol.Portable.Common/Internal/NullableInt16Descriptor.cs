using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(short?))]
public sealed partial class NullableInt16Descriptor : ArithmeticTypeDescriptor
{
    public override IReadOnlyList<PropertyInfo> Properties { get; } = new PropertyInfo[]
    {
        (PropertyInfo)((MemberExpression)((Expression<Func<short?, bool>>)(e => e.HasValue)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<short?, short>>)(e => e!.Value)).Body).Member
    };

    public override bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(short?));

    public override object Parse(string value)
        => short.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public override string? Stringify(object? value) => value is null ? default : ((short)value!).ToString("D", CultureInfo.InvariantCulture);
}