using System;
using System.Globalization;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(short?))]
public sealed partial class NullableInt16Descriptor : ArithmeticTypeDescriptor
{
    public override bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(short?));

    public override object Parse(string value)
        => short.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public override string? Stringify(object? value) => value is null ? default : ((short)value!).ToString("D", CultureInfo.InvariantCulture);
}