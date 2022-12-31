using System;
using System.Globalization;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(long?))]
public sealed partial class NullableInt64Descriptor : ArithmeticTypeDescriptor
{
    public override bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(long?));

    public override object Parse(string value)
        => long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public override string? Stringify(object? value) => value is null ? default : ((long)value!).ToString("D", CultureInfo.InvariantCulture);
}