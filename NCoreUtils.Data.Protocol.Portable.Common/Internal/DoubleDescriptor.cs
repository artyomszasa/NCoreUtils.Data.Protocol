using System.Globalization;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(double))]
public sealed partial class DoubleDescriptor : ArithmeticTypeDescriptor
{
    public override object Parse(string value)
        => double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);

    public override string Stringify(object? value) => ((double)value!).ToString("G", CultureInfo.InvariantCulture);

    public override bool IsAssignableTo(Type baseType)
        => baseType == typeof(double) || baseType == typeof(double?);
}