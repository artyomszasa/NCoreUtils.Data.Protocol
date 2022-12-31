using System;
using System.Globalization;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(ulong))]
public sealed partial class UInt64Descriptor : ArithmeticTypeDescriptor
{
    public override object Parse(string value)
        => ulong.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public override string Stringify(object? value) => ((ulong)value!).ToString("D", CultureInfo.InvariantCulture);

    public override bool IsAssignableTo(Type baseType)
        => baseType == typeof(ulong) || baseType == typeof(ulong);
}