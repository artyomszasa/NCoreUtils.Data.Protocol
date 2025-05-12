using System;
using System.Globalization;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(float))]
public sealed partial class SingleDescriptor : ArithmeticTypeDescriptor
{
    public override object Parse(string value)
        => float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);

    public override string Stringify(object? value) => ((float)value!).ToString("G", CultureInfo.InvariantCulture);

    public override bool IsAssignableTo(Type baseType)
        => baseType == typeof(float) || baseType == typeof(float?)
            || baseType == typeof(double) || baseType == typeof(double?);
}