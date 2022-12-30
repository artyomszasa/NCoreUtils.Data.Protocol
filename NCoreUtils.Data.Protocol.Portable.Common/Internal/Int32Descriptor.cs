using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

public sealed class Int32Descriptor : ArithmeticTypeDescriptor
{
    public sealed class Box
    {
        public readonly int Value;

        public Box(int value) => Value = value;

        public override string ToString() => $"{{{Value}}}";
    }

    private FieldInfo BoxValueField { get; } = (FieldInfo)((MemberExpression)((Expression<Func<Box, int>>)(e => e.Value)).Body).Member;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public override Type Type => typeof(int);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public override Type ArrayOfType => typeof(int[]);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public override Type EnumerableOfType => typeof(IEnumerable<int>);

    public override object? BoxNullable(object value)
        => (int?)(int)value;

    public override object Parse(string value)
        => int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public override string Stringify(object? value) => ((int)value!).ToString("D", CultureInfo.InvariantCulture);

    public override Expression CreateBoxedConstant(object? value)
        => Expression.Field(
            Expression.Constant(new Box((int)value!)),
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
                Expression.Convert(self, typeof(long?)),
                Expression.Convert(right, typeof(long?))
            );
        }
        if (typeof(int?) == right.Type || typeof(short?) == right.Type || typeof(ushort?) == right.Type)
        {
            return (
                Expression.Convert(self, typeof(int?)),
                Expression.Convert(right, typeof(int?))
            );
        }
        throw new InvalidOperationException($"Unable to unify types Int32 and {right.Type}.");
    }

    public override bool IsAssignableTo(Type baseType)
        => baseType == typeof(int);

    public override MethodInfo EnumerableAnyMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<int>, Func<int, bool>, bool>(Enumerable.Any);

    public override MethodInfo EnumerableAllMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<int>, Func<int, bool>, bool>(Enumerable.All);

    public override MethodInfo EnumerableContainsMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<int>, int, bool>(Enumerable.Contains);

    public override void Accept(IDataTypeVisitor visitor)
        => visitor.Visit<int>();
}