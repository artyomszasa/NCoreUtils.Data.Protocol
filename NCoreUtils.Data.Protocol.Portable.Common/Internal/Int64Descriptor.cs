using System;
using System.Globalization;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(long))]
public sealed partial class Int64Descriptor : ArithmeticTypeDescriptor
{
    public override object Parse(string value)
        => long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public override string Stringify(object? value) => ((long)value!).ToString("D", CultureInfo.InvariantCulture);

    public override bool IsAssignableTo(Type baseType)
        => baseType == typeof(long) || baseType == typeof(long?);
}