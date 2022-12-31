using System;
using System.Globalization;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(ushort))]
public sealed partial class UInt16Descriptor : ArithmeticTypeDescriptor
{
    public override object Parse(string value)
        => ushort.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public override string Stringify(object? value) => ((ushort)value!).ToString("D", CultureInfo.InvariantCulture);

    public override bool IsAssignableTo(Type baseType)
        => baseType == typeof(ushort) || baseType == typeof(ushort?)
            || baseType == typeof(int) || baseType == typeof(int?)
            || baseType == typeof(uint) || baseType == typeof(uint?)
            || baseType == typeof(long) || baseType == typeof(long?)
            || baseType == typeof(ulong) || baseType == typeof(ulong?);
}