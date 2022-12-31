using System;
using System.Globalization;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(uint))]
public sealed partial class UInt32Descriptor : ArithmeticTypeDescriptor
{
    public override object Parse(string value)
        => uint.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public override string Stringify(object? value) => ((uint)value!).ToString("D", CultureInfo.InvariantCulture);

    public override bool IsAssignableTo(Type baseType)
        => baseType == typeof(uint) || baseType == typeof(uint?)
            || baseType == typeof(long) || baseType == typeof(long?)
            || baseType == typeof(ulong) || baseType == typeof(ulong?);
}