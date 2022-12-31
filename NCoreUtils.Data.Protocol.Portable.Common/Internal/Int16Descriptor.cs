using System;
using System.Globalization;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(short))]
public sealed partial class Int16Descriptor : ArithmeticTypeDescriptor
{
    public override object Parse(string value)
        => short.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public override string Stringify(object? value) => ((short)value!).ToString("D", CultureInfo.InvariantCulture);

    public override bool IsAssignableTo(Type baseType)
        => baseType == typeof(short) || baseType == typeof(short?)
            || baseType == typeof(int) || baseType == typeof(int?)
            || baseType == typeof(long) || baseType == typeof(long?);
}