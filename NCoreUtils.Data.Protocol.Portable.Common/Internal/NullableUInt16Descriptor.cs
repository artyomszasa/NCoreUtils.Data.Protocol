using System;
using System.Globalization;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(ushort?))]
public sealed partial class NullableUInt16Descriptor : ArithmeticTypeDescriptor
{
    public override bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(ushort?));

    public override object Parse(string value)
        => ushort.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public override string? Stringify(object? value) => value is null ? default : ((ushort)value!).ToString("D", CultureInfo.InvariantCulture);
}