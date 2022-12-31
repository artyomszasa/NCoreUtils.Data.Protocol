using System;
using System.Linq.Expressions;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptorAttribute(typeof(Guid))]
public sealed partial class GuidDescriptor : ITypeDescriptor
{
    public bool IsArithmetic => false;

    public bool IsEnum => false;

    public bool IsValue => true;

    public Expression CreateEqual(Expression self, Expression right)
        => right.Type == typeof(Guid)
            ? Expression.Equal(self, right)
            : right.Type == typeof(Guid?)
                ? Expression.Equal(Expression.Convert(self, typeof(Guid?)), right)
                : throw new InvalidOperationException($"Cannot create Equal expression from Guid and {right.Type}.");

    public Expression CreateNotEqual(Expression self, Expression right)
        => right.Type == typeof(Guid)
            ? Expression.NotEqual(self, right)
            : right.Type == typeof(Guid?)
                ? Expression.NotEqual(Expression.Convert(self, typeof(Guid?)), right)
                : throw new InvalidOperationException($"Cannot create NotEqual expression from Guid and {right.Type}.");

    public bool IsAssignableTo(Type baseType)
        => baseType == typeof(Guid) || baseType == typeof(Guid?);

    public object Parse(string value) => string.IsNullOrEmpty(value) ? default : Guid.Parse(value);

    public string Stringify(object? value) => ((Guid)value!).ToString();
}