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
    public static ExpressionBuilderVisitor Singleton { get; } = new ExpressionBuilderVisitor();

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

    public BinaryExpression VisitBinary(Binary<Type> node, IPropertyResolver propertyResolver, NameMap nameMap)
    {
        var left = node.Left.Accept(this, propertyResolver, nameMap);
        var right = node.Right.Accept(this, propertyResolver, nameMap);
        return node.Operation switch
        {
            BinaryOperation.AndAlso            => Expression.AndAlso            (left, right),
            BinaryOperation.OrElse             => Expression.OrElse             (left, right),
            BinaryOperation.Equal              => Expression.Equal              (left, right),
            BinaryOperation.NotEqual           => Expression.NotEqual           (left, right),
            BinaryOperation.GreaterThan        => Expression.GreaterThan        (left, right),
            BinaryOperation.GreaterThanOrEqual => Expression.GreaterThanOrEqual (left, right),
            BinaryOperation.LessThan           => Expression.LessThan           (left, right),
            BinaryOperation.LessThanOrEqual    => Expression.LessThanOrEqual    (left, right),
            BinaryOperation.Add                => Expression.Add                (left, right),
            BinaryOperation.Substract          => Expression.Subtract           (left, right),
            BinaryOperation.Multiply           => Expression.Multiply           (left, right),
            BinaryOperation.Divide             => Expression.Divide             (left, right),
            BinaryOperation.Modulo             => Expression.Modulo             (left, right),
            var op => throw new NotSupportedException($"Not supported binary operation {op}.")
        };
    }

    public Expression VisitCall(Call<Type> node, IPropertyResolver propertyResolver, NameMap nameMap)
        => node.Descriptor.CreateExpression(
            node.Arguments.MapToArray(arg => arg.Accept(this, propertyResolver, nameMap))
        );

    public Expression VisitConstant(Constant<Type> node, IPropertyResolver propertyResolver, NameMap nameMap)
    {
        if (node.RawValue is null)
        {
            if (node.Type.IsValueType && !node.Type.IsOptionalValue())
            {
                throw new InvalidOperationException("Null value cannot be used with value types.");
            }
            return Expression.Constant(null, node.Type);
        }
        if (CommonConstantVisitors.TryGetValue(node.Type, out var visitor))
        {
            return visitor(node.RawValue);
        }
        if (node.Type.IsEnum)
        {
            if (long.TryParse(node.RawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i64))
            {
                // TODO: optimize
                var value = Convert.ChangeType(i64, Enum.GetUnderlyingType(node.Type));
                return BoxedConstantBuilder.BuildExpression(Enum.ToObject(node.Type, value), node.Type);
            }
            return BoxedConstantBuilder.BuildExpression(Enum.Parse(node.Type, node.RawValue), node.Type);
        }
        return BoxedConstantBuilder.BuildExpression(ChangeType(node.RawValue, node.Type), node.Type);
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