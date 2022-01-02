using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using NCoreUtils.Data.Protocol.Ast;
using NCoreUtils.Data.Protocol.TypeInference;
using NCoreUtils.Data.Protocol.TypeInference.Ast;

namespace NCoreUtils.Data.Protocol.Internal;

public partial class ExpressionBuilderVisitor : ITypedNodeVisitor<Type, IPropertyResolver, NameMap, Expression>
{
    private struct ConstantData
    {
        public object? Value { get; }

        public Type Type { get; }

        public bool ShouldBox => Value is not null;

        public ConstantData(object? value, Type type)
        {
            Value = value;
            Type = type;
        }

        public Expression BuildExpression()
            => ShouldBox
                ? BoxedConstantBuilder.BuildExpression(Value, Type)
                : Expression.Constant(Value, Type);
    }

    public static ExpressionBuilderVisitor Singleton { get; } = new ExpressionBuilderVisitor();

    private static bool IsNullable(Type type, [MaybeNullWhen(false)] out Type innerType)
    {
        if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            innerType = type.GetGenericArguments()[0];
            return true;
        }
        innerType = default;
        return false;
    }

    private static object ChangeType(string value, Type targetType)
    {
        if (targetType == typeof(Guid))
        {
            return value.Length == 0
                ? Guid.Empty
                : Guid.Parse(value);
        }
        return Convert.ChangeType(value, targetType);
    }

    private ExpressionBuilderVisitor() { }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "All types in expression are should be preserved outside this context.")]
    private ConstantData VisitConstant(Type type, string? rawValue)
    {
        if (rawValue is null)
        {
            if (type.IsValueType && !type.IsOptionalValue())
            {
                throw new InvalidOperationException("Null value cannot be used with value types.");
            }
            return new(null, type);
        }
        if (type.IsEnum)
        {
            if (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i64))
            {
                // TODO: optimize
                var value = Convert.ChangeType(i64, Enum.GetUnderlyingType(type));
                return new(Enum.ToObject(type, value), type);
            }
            return new(Enum.Parse(type, rawValue, true), type);
        }
        if (IsNullable(type, out var innerType))
        {
            var innerValue = VisitConstant(innerType, rawValue);
            return new(
                value: Activator.CreateInstance(typeof(Nullable<>).MakeGenericType(innerType), innerValue.Value)!,
                type: type
            );
        }
        return new(ChangeType(rawValue, type), type);
    }

    public BinaryExpression VisitBinary(Binary<Type> node, IPropertyResolver propertyResolver, NameMap nameMap)
    {
        var left = node.Left.Accept(this, propertyResolver, nameMap);
        var right = node.Right.Accept(this, propertyResolver, nameMap);
        return node.Operation switch
        {
            BinaryOperation.AndAlso            => Expression.AndAlso       (left, right),
            BinaryOperation.OrElse             => Expression.OrElse        (left, right),
            BinaryOperation.Equal              => CreateEqual              (left, right),
            BinaryOperation.NotEqual           => CreateNotEqual           (left, right),
            BinaryOperation.GreaterThan        => CreateGreaterThan        (left, right),
            BinaryOperation.GreaterThanOrEqual => CreateGreaterThanOrEqual (left, right),
            BinaryOperation.LessThan           => CreateLessThan           (left, right),
            BinaryOperation.LessThanOrEqual    => CreateLessThanOrEqual    (left, right),
            BinaryOperation.Add                => Expression.Add           (left, right),
            BinaryOperation.Substract          => Expression.Subtract      (left, right),
            BinaryOperation.Multiply           => Expression.Multiply      (left, right),
            BinaryOperation.Divide             => Expression.Divide        (left, right),
            BinaryOperation.Modulo             => Expression.Modulo        (left, right),
            var op => throw new NotSupportedException($"Not supported binary operation {op}.")
        };
    }

    public Expression VisitCall(Call<Type> node, IPropertyResolver propertyResolver, NameMap nameMap)
        => node.Descriptor.CreateExpression(
            node.Arguments.MapToArray(arg => arg.Accept(this, propertyResolver, nameMap))
        );

    public Expression VisitConstant(Constant<Type> node, IPropertyResolver propertyResolver, NameMap nameMap)
    {
        if (node.RawValue is not null && CommonConstantVisitors.TryGetValue(node.Type, out var visitor))
        {
            return visitor(node.RawValue);
        }
        return VisitConstant(node.Type, node.RawValue).BuildExpression();
    }

    [SuppressMessage("Style", "IDE0060", MessageId = "propertyResolver")]
    [SuppressMessage("Performance", "CA1822")]
    public ParameterExpression VisitIdentifier(Identifier<Type> identifier, IPropertyResolver propertyResolver, NameMap nameMap)
        => nameMap.GetParameter(identifier.Value);

    public LambdaExpression VisitLambda(Lambda<Type> lambda, IPropertyResolver propertyResolver, NameMap nameMap)
    {
        var parameter = nameMap.Add(lambda.Arg.Value, lambda.Arg.Type);
        var body = lambda.Body.Accept(this, propertyResolver, nameMap);
        return Expression.Lambda(body, parameter);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "Types are checked during type inference.")]
    public Expression VisitMember(Member<Type> member, IPropertyResolver propertyResolver, NameMap nameMap)
    {
        var instance = member.Instance.Accept(this, propertyResolver, nameMap);
        if (!propertyResolver.TryResolveProperty(instance.Type, member.MemberName, out var property))
        {
            property = DefaultPropertyResolver.Singleton.ResolveProperty(instance.Type, member.MemberName);
        }
        return property.CreateExpression(instance);
    }

    Expression ITypedNodeVisitor<Type, IPropertyResolver, NameMap, Expression>.VisitBinary(Binary<Type> binary, IPropertyResolver propertyResolver, NameMap nameMap)
        => VisitBinary(binary, propertyResolver, nameMap);

    Expression ITypedNodeVisitor<Type, IPropertyResolver, NameMap, Expression>.VisitIdentifier(Identifier<Type> identifier, IPropertyResolver propertyResolver, NameMap nameMap)
        => VisitIdentifier(identifier, propertyResolver, nameMap);

    Expression ITypedNodeVisitor<Type, IPropertyResolver, NameMap, Expression>.VisitLambda(Lambda<Type> lambda, IPropertyResolver propertyResolver, NameMap nameMap)
        => VisitLambda(lambda, propertyResolver, nameMap);
}