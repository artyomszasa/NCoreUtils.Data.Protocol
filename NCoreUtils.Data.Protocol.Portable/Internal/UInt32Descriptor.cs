using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

public sealed class UInt32Descriptor : ArithmeticTypeDescriptor
{
    public sealed class Box
    {
        public readonly uint Value;

        public Box(uint value) => Value = value;
    }

    private FieldInfo BoxValueField { get; } = (FieldInfo)((MemberExpression)((Expression<Func<Box, uint>>)(e => e.Value)).Body).Member;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public override Type Type => typeof(uint);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public override Type ArrayOfType => typeof(uint[]);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public override Type EnumerableOfType => typeof(IEnumerable<uint>);

    public override object? BoxNullable(object value)
        => (uint?)(uint)value;

    public override object Parse(string value)
        => uint.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public override string Stringify(object? value) => ((uint)value!).ToString("D", CultureInfo.InvariantCulture);

    public override Expression CreateBoxedConstant(object? value)
        => Expression.Field(
            Expression.Constant(new Box((uint)value!)),
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
        if (typeof(int) == right.Type)
        {
            return (
                Expression.Convert(self, typeof(long)),
                Expression.Convert(right, typeof(long))
            );
        }
        if (typeof(uint) == right.Type)
        {
            return (self, right);
        }
        if (typeof(short) == right.Type || typeof(ushort) == right.Type)
        {
            return (self, Expression.Convert(right, typeof(int)));
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
                Expression.Convert(self, typeof(uint?)),
                Expression.Convert(right, typeof(uint?))
            );
        }
        if (typeof(int?) == right.Type)
        {
            return (
                Expression.Convert(self, typeof(long?)),
                Expression.Convert(right, typeof(long?))
            );
        }
        if (typeof(short?) == right.Type || typeof(ushort?) == right.Type)
        {
            return (
                Expression.Convert(self, typeof(int?)),
                Expression.Convert(right, typeof(int?))
            );
        }
        throw new InvalidOperationException($"Unable to unify types UInt32 and {right.Type}.");
    }

    public override bool IsAssignableTo(Type baseType)
        => baseType == typeof(uint);

    public override MethodInfo EnumerableAnyMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<uint>, Func<uint, bool>, bool>(Enumerable.Any);

    public override MethodInfo EnumerableAllMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<uint>, Func<uint, bool>, bool>(Enumerable.All);
}