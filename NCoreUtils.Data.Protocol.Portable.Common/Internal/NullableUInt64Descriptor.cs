using System;
using System.Globalization;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(ulong?))]
public sealed partial class NullableUInt64Descriptor : ArithmeticTypeDescriptor
{
    public override bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(ulong?));

    public override object Parse(string value)
        => ulong.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public override string? Stringify(object? value) => value is null ? default : ((ulong)value!).ToString("D", CultureInfo.InvariantCulture);
}