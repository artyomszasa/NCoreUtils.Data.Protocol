using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

public sealed class Int16Descriptor : ArithmeticTypeDescriptor
{
    public sealed class Box
    {
        public readonly short Value;

        public Box(short value) => Value = value;
    }

    private FieldInfo BoxValueField { get; } = (FieldInfo)((MemberExpression)((Expression<Func<Box, short>>)(e => e.Value)).Body).Member;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public override Type Type => typeof(short);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public override Type ArrayOfType => typeof(short[]);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public override Type EnumerableOfType => typeof(IEnumerable<short>);

    public override object? BoxNullable(object value)
        => (short?)(short)value;

    public override object Parse(string value)
        => short.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public override string Stringify(object? value) => ((short)value!).ToString("D", CultureInfo.InvariantCulture);

    public override Expression CreateBoxedConstant(object? value)
        => Expression.Field(
            Expression.Constant(new Box((short)value!)),
            BoxValueField
        );

    protected override (Expression Left, Expression Right) UnifyExpressionTypes(Expression self, Expression right)
    {
        if (typeof(long) == right.Type)
        {
            return (
                Expression.Convert(self, typeof(long)),
                right
            );
        }
        if (typeof(uint) == right.Type)
        {
            return (
                Expression.Convert(self, typeof(long)),
                Expression.Convert(right, typeof(long))
            );
        }
        if (typeof(int) == right.Type)
        {
            return (Expression.Convert(self, typeof(int)), right);
        }
        if (typeof(ushort) == right.Type)
        {
            return (Expression.Convert(self, typeof(int)), Expression.Convert(right, typeof(int)));
        }
        if (typeof(short) == right.Type)
        {
            return (self, right);
        }
        if (typeof(long?) == right.Type)
        {
            return (
                Expression.Convert(self, typeof(long?)),
                right
            );
        }
        if (typeof(uint?) == right.Type)
        {
            return (
                Expression.Convert(self, typeof(long?)),
                Expression.Convert(right, typeof(long?))
            );
        }
        if (typeof(int?) == right.Type || typeof(ushort?) == right.Type)
        {
            return (
                Expression.Convert(self, typeof(int?)),
                Expression.Convert(right, typeof(int?))
            );
        }
        if (typeof(short?) == right.Type)
        {
            return (
                Expression.Convert(self, typeof(short?)),
                Expression.Convert(right, typeof(short?))
            );
        }
        throw new InvalidOperationException($"Unable to unify types Int16 and {right.Type}.");
    }

    public override bool IsAssignableTo(Type baseType)
        => baseType == typeof(short);

    public override MethodInfo EnumerableAnyMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<short>, Func<short, bool>, bool>(Enumerable.Any);

    public override MethodInfo EnumerableAllMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<short>, Func<short, bool>, bool>(Enumerable.All);

    public override void Accept(IDataTypeVisitor visitor)
        => visitor.Visit<short>();
}