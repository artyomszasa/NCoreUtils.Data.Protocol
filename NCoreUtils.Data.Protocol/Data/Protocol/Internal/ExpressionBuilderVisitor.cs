using System;
using System.Collections.Concurrent;
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

        public Expression BuildExpression(IDataUtils util)
            => ShouldBox
                ? util.CreateBoxedConstant(Type, Value)
                : Expression.Constant(Value, Type);
    }

    private static ConcurrentDictionary<IDataUtils, ExpressionBuilderVisitor>? _instanceCache;

    private static Func<IDataUtils, ExpressionBuilderVisitor>? _instanceFactory;

    private static ConcurrentDictionary<IDataUtils, ExpressionBuilderVisitor> InstanceCache
        => _instanceCache ??= new();

    private static Func<IDataUtils, ExpressionBuilderVisitor> InstanceFactory
        => _instanceFactory ??= static util => new(util);

    public static ExpressionBuilderVisitor For(IDataUtils util)
        => InstanceCache.GetOrAdd(util, InstanceFactory);

    private IDataUtils Util { get; }

    private ExpressionBuilderVisitor(IDataUtils util)
        => Util = util ?? throw new ArgumentNullException(nameof(util));

    private ConstantData VisitConstant(Type type, string? rawValue)
    {
        if (rawValue is null)
        {
            if (Util.IsValue(type) && !Util.IsOptional(type))
            {
                throw new InvalidOperationException("Null value cannot be used with value types.");
            }
            return new(null, type);
        }
        if (Util.TryGetEnumFactory(type, out var enumFactory))
        {
            return new(enumFactory.FromRawValue(rawValue), type);
        }
        if (Util.IsNullable(type, out var innerType))
        {
            var innerValue = VisitConstant(innerType, rawValue);
            return new(
                value: Util.BoxNullable(innerType, innerValue.Value!),
                type: type
            );
        }
        return new(Util.Parse(type, rawValue), type);
    }

    public Expression VisitBinary(Binary<Type> node, IPropertyResolver propertyResolver, NameMap nameMap)
    {
        var left = node.Left.Accept(this, propertyResolver, nameMap);
        var right = node.Right.Accept(this, propertyResolver, nameMap);
        return node.Operation switch
        {
            BinaryOperation.AndAlso            => Util.CreateAndAlso            (left, right),
            BinaryOperation.OrElse             => Util.CreateOrElse             (left, right),
            BinaryOperation.Equal              => Util.CreateEqual              (left, right),
            BinaryOperation.NotEqual           => Util.CreateNotEqual           (left, right),
            BinaryOperation.GreaterThan        => Util.CreateGreaterThan        (left, right),
            BinaryOperation.GreaterThanOrEqual => Util.CreateGreaterThanOrEqual (left, right),
            BinaryOperation.LessThan           => Util.CreateLessThan           (left, right),
            BinaryOperation.LessThanOrEqual    => Util.CreateLessThanOrEqual    (left, right),
            BinaryOperation.Add                => Util.CreateAdd                (left, right),
            BinaryOperation.Subtract           => Util.CreateSubtract           (left, right),
            BinaryOperation.Multiply           => Util.CreateMultiply           (left, right),
            BinaryOperation.Divide             => Util.CreateDivide             (left, right),
            BinaryOperation.Modulo             => Util.CreateModulo             (left, right),
            var op => throw new NotSupportedException($"Not supported binary operation {op}.")
        };
    }

    public Expression VisitCall(Call<Type> node, IPropertyResolver propertyResolver, NameMap nameMap)
        => node.Descriptor.CreateExpression(
            node.Arguments.MapToArray(arg => arg.Accept(this, propertyResolver, nameMap))
        );

    public Expression VisitConstant(Constant<Type> node, IPropertyResolver propertyResolver, NameMap nameMap)
        => VisitConstant(node.Type, node.RawValue).BuildExpression(Util);

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
            // NOTE: fallback
            property = DefaultPropertyResolver.For(Util).ResolveProperty(instance.Type, member.MemberName);
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