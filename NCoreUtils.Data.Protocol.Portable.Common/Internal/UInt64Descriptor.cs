using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

public sealed class UInt64Descriptor : ArithmeticTypeDescriptor
{
    public sealed class Box
    {
        public readonly ulong Value;

        public Box(ulong value) => Value = value;

        public override string ToString() => $"{{{Value}}}";
    }

    private FieldInfo BoxValueField { get; } = (FieldInfo)((MemberExpression)((Expression<Func<Box, ulong>>)(e => e.Value)).Body).Member;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public override Type Type => typeof(ulong);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public override Type ArrayOfType => typeof(ulong[]);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public override Type EnumerableOfType => typeof(IEnumerable<ulong>);

    public override object? BoxNullable(object value)
        => (ulong?)(ulong)value;

    public override object Parse(string value)
        => ulong.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public override string Stringify(object? value) => ((ulong)value!).ToString("D", CultureInfo.InvariantCulture);

    public override Expression CreateBoxedConstant(object? value)
        => Expression.Field(
            Expression.Constant(new Box((ulong)value!)),
            BoxValueField
        );

    protected override (Expression Left, Expression Right) UnifyExpressionTypes(Expression self, Expression right)
    {
        if (typeof(ulong) == right.Type)
        {
            return (self, right);
        }
        if (typeof(uint) == right.Type || typeof(ushort) == right.Type)
        {
            return (self, Expression.Convert(right, typeof(ulong)));
        }
        if (typeof(ulong?) == right.Type)
        {
            return (
                Expression.Convert(self, typeof(ulong?)),
                right
            );
        }
        if (typeof(uint?) == right.Type || typeof(ushort?) == right.Type)
        {
            return (
                Expression.Convert(self, typeof(ulong?)),
                Expression.Convert(right, typeof(ulong?))
            );
        }
        throw new InvalidOperationException($"Unable to unify types UInt64 and {right.Type}.");
    }

    public override bool IsAssignableTo(Type baseType)
        => baseType == typeof(ulong);

    public override MethodInfo EnumerableAnyMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<ulong>, Func<ulong, bool>, bool>(Enumerable.Any);

    public override MethodInfo EnumerableAllMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<ulong>, Func<ulong, bool>, bool>(Enumerable.All);

    public override MethodInfo EnumerableContainsMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<ulong>, ulong, bool>(Enumerable.Contains);

    public override void Accept(IDataTypeVisitor visitor)
        => visitor.Visit<ulong>();
}