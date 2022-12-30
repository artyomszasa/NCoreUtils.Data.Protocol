using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

public sealed class Int64Descriptor : ArithmeticTypeDescriptor
{
    public sealed class Box
    {
        public readonly long Value;

        public Box(long value) => Value = value;
    }

    private FieldInfo BoxValueField { get; } = (FieldInfo)((MemberExpression)((Expression<Func<Box, long>>)(e => e.Value)).Body).Member;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public override Type Type => typeof(long);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public override Type ArrayOfType => typeof(long[]);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public override Type EnumerableOfType => typeof(IEnumerable<long>);

    public override object? BoxNullable(object value)
        => (long?)(long)value;

    public override object Parse(string value)
        => long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public override string Stringify(object? value) => ((long)value!).ToString("D", CultureInfo.InvariantCulture);

    public override Expression CreateBoxedConstant(object? value)
        => Expression.Field(
            Expression.Constant(new Box((long)value!)),
            BoxValueField
        );

    protected override (Expression Left, Expression Right) UnifyExpressionTypes(Expression self, Expression right)
    {
        if (typeof(long) == right.Type)
        {
            return (self, right);
        }
        if (typeof(uint) == right.Type || typeof(int) == right.Type || typeof(short) == right.Type || typeof(ushort) == right.Type)
        {
            return (self, Expression.Convert(right, typeof(long)));
        }
        if (typeof(long?) == right.Type)
        {
            return (
                Expression.Convert(self, typeof(long?)),
                right
            );
        }
        if (typeof(uint?) == right.Type || typeof(int?) == right.Type || typeof(short?) == right.Type || typeof(ushort?) == right.Type)
        {
            return (
                Expression.Convert(self, typeof(long?)),
                Expression.Convert(right, typeof(long?))
            );
        }
        throw new InvalidOperationException($"Unable to unify types Int64 and {right.Type}.");
    }

    public override bool IsAssignableTo(Type baseType)
        => baseType == typeof(long);

    public override MethodInfo EnumerableAnyMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<long>, Func<long, bool>, bool>(Enumerable.Any);

    public override MethodInfo EnumerableAllMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<long>, Func<long, bool>, bool>(Enumerable.All);

    public override void Accept(IDataTypeVisitor visitor)
        => visitor.Visit<long>();
}