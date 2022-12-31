using System;
using System.Globalization;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(int))]
public sealed partial class Int32Descriptor : ArithmeticTypeDescriptor
{
    public override object Parse(string value)
        => int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public override string Stringify(object? value) => ((int)value!).ToString("D", CultureInfo.InvariantCulture);

    public override bool IsAssignableTo(Type baseType)
        => baseType == typeof(int) || baseType == typeof(int?)
            || baseType == typeof(long) || baseType == typeof(long?);
}