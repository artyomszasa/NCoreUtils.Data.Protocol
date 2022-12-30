using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

public sealed class UInt16Descriptor : ArithmeticTypeDescriptor
{
    public sealed class Box
    {
        public readonly ushort Value;

        public Box(ushort value) => Value = value;
    }

    private FieldInfo BoxValueField { get; } = (FieldInfo)((MemberExpression)((Expression<Func<Box, ushort>>)(e => e.Value)).Body).Member;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public override Type Type => typeof(ushort);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public override Type ArrayOfType => typeof(ushort[]);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public override Type EnumerableOfType => typeof(IEnumerable<ushort>);

    public override object? BoxNullable(object value)
        => (ushort?)(ushort)value;

    public override object Parse(string value)
        => ushort.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public override string Stringify(object? value) => ((ushort)value!).ToString("D", CultureInfo.InvariantCulture);

    public override Expression CreateBoxedConstant(object? value)
        => Expression.Field(
            Expression.Constant(new Box((ushort)value!)),
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
        if (typeof(short) == right.Type)
        {
            return (Expression.Convert(self, typeof(int)), Expression.Convert(right, typeof(int)));
        }
        if (typeof(ushort) == right.Type)
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
        if (typeof(int?) == right.Type || typeof(short?) == right.Type)
        {
            return (
                Expression.Convert(self, typeof(int?)),
                Expression.Convert(right, typeof(int?))
            );
        }
        if (typeof(ushort?) == right.Type)
        {
            return (
                Expression.Convert(self, typeof(ushort?)),
                Expression.Convert(right, typeof(ushort?))
            );
        }
        throw new InvalidOperationException($"Unable to unify types UInt16 and {right.Type}.");
    }

    public override bool IsAssignableTo(Type baseType)
        => baseType == typeof(ushort);

    public override MethodInfo EnumerableAnyMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<ushort>, Func<ushort, bool>, bool>(Enumerable.Any);

    public override MethodInfo EnumerableAllMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<ushort>, Func<ushort, bool>, bool>(Enumerable.All);

    public override void Accept(IDataTypeVisitor visitor)
        => visitor.Visit<ushort>();
}