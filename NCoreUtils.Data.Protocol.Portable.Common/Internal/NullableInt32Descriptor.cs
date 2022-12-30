using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

public sealed class NullableInt32Descriptor : ArithmeticTypeDescriptor
{
    public sealed class Box
    {
        public readonly int? Value;

        public Box(int? value) => Value = value;
    }

    private FieldInfo BoxValueField { get; } = (FieldInfo)((MemberExpression)((Expression<Func<Box, int?>>)(e => e.Value)).Body).Member;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public override Type Type => typeof(int?);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public override Type ArrayOfType => typeof(int?[]);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public override Type EnumerableOfType => typeof(IEnumerable<int?>);

    public override IReadOnlyList<PropertyInfo> Properties { get; } = new []
    {
        (PropertyInfo)((MemberExpression)((Expression<Func<int?, int>>)(e => e!.Value)).Body).Member,
        (PropertyInfo)((MemberExpression)((Expression<Func<int?, bool>>)(e => e.HasValue)).Body).Member
    };

    public override object? BoxNullable(object value)
        => throw new InvalidOperationException("Unable to create nullable from nullable.");

    public override bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(int?));

    public override bool IsNullable([MaybeNullWhen(false)] out Type elementType)
    {
        elementType = typeof(int);
        return true;
    }

    public override object Parse(string value)
        => int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public override string? Stringify(object? value) => value is null ? default : ((int)value!).ToString("D", CultureInfo.InvariantCulture);

    public override Expression CreateBoxedConstant(object? value)
        => Expression.Field(
            Expression.Constant(new Box((int?)value)),
            BoxValueField
        );

    protected override (Expression Left, Expression Right) UnifyExpressionTypes(Expression self, Expression right)
    {
        if (typeof(long?) == right.Type)
        {
            // upcast self
            return (
                Expression.Convert(self, typeof(long?)),
                right
            );
        }
        if (typeof(int?) == right.Type)
        {
            // no conversion
            return (self, right);
        }
        if (typeof(long) == right.Type || typeof(uint) == right.Type || typeof(uint?) == right.Type)
        {
            // upcast both
            return (
                Expression.Convert(self, typeof(long?)),
                Expression.Convert(right, typeof(long?))
            );
        }
        if (typeof(int) == right.Type || typeof(short) == right.Type || typeof(ushort) == right.Type || typeof(short?) == right.Type || typeof(ushort?) == right.Type)
        {
            // upcast right
            return (
                self,
                Expression.Convert(right, typeof(int?))
            );
        }
        throw new InvalidOperationException($"Unable to unify types Nullable<Int32> and {right.Type}.");
    }

    public override MethodInfo EnumerableAnyMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<int?>, Func<int?, bool>, bool>(Enumerable.Any);

    public override MethodInfo EnumerableAllMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<int?>, Func<int?, bool>, bool>(Enumerable.All);

    public override void Accept(IDataTypeVisitor visitor)
        => visitor.Visit<int?>();
}