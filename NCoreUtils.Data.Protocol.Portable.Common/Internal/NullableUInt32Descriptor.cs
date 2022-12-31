using System;
using System.Globalization;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(uint?))]
public sealed partial class NullableUInt32Descriptor : ArithmeticTypeDescriptor
{
    public override bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(uint?));

    public override object Parse(string value)
        => uint.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public override string? Stringify(object? value) => value is null ? default : ((uint)value!).ToString("D", CultureInfo.InvariantCulture);
}