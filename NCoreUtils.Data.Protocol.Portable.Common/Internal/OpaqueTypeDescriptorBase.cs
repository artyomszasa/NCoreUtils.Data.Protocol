using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

public abstract class OpaqueTypeDescriptorBase
    : ITypeDescriptor
{
    public IReadOnlyList<PropertyInfo> Properties { get; } = Array.Empty<PropertyInfo>();

    public bool IsArithmetic => false;

    public bool IsEnum => false;

    public abstract bool IsValue { get; }

    public abstract Type Type { get; }
    public abstract Type ArrayOfType { get; }
    public abstract Type EnumerableOfType { get; }
    public abstract MethodInfo EnumerableAnyMethod { get; }
    public abstract MethodInfo EnumerableAllMethod { get; }
    public abstract MethodInfo EnumerableContainsMethod { get; }

    public object? BoxNullable(object value)
    {
        throw new NotSupportedException();
    }

    public bool IsEnumerable([MaybeNullWhen(false)] out Type elementType)
    {
        elementType = default;
        return false;
    }

    public bool IsArray([MaybeNullWhen(false)] out Type elementType)
    {
        elementType = default;
        return false;
    }

    public bool IsLambda([MaybeNullWhen(false)] out Type argType, [MaybeNullWhen(false)] out Type resType)
    {
        argType = default;
        resType = default;
        return false;
    }

    public bool IsMaybe([MaybeNullWhen(false)] out Type elementType)
    {
        elementType = default;
        return false;
    }

    public bool IsNullable([MaybeNullWhen(false)] out Type elementType)
    {
        elementType = default;
        return false;
    }

    public bool TryGetEnumFactory([MaybeNullWhen(false)] out IEnumFactory enumFactory)
    {
        enumFactory = default;
        return false;
    }

    public Expression CreateAdd(Expression self, Expression right)
    {
        throw new NotSupportedException();
    }

    public Expression CreateAndAlso(Expression self, Expression right)
    {
        throw new NotSupportedException();
    }

    public Expression CreateDivide(Expression self, Expression right)
    {
        throw new NotSupportedException();
    }

    public Expression CreateEqual(Expression self, Expression right)
    {
        return Expression.Equal(self, right);
    }

    public Expression CreateGreaterThan(Expression self, Expression right)
    {
        throw new NotSupportedException();
    }

    public Expression CreateGreaterThanOrEqual(Expression self, Expression right)
    {
        throw new NotSupportedException();
    }

    public Expression CreateLessThan(Expression self, Expression right)
    {
        throw new NotSupportedException();
    }

    public Expression CreateLessThanOrEqual(Expression self, Expression right)
    {
        throw new NotSupportedException();
    }

    public Expression CreateModulo(Expression self, Expression right)
    {
        throw new NotSupportedException();
    }

    public Expression CreateMultiply(Expression self, Expression right)
    {
        throw new NotSupportedException();
    }

    public Expression CreateNotEqual(Expression self, Expression right)
    {
        return Expression.NotEqual(self, right);
    }

    public Expression CreateOrElse(Expression self, Expression right)
    {
        throw new NotSupportedException();
    }

    public Expression CreateSubtract(Expression self, Expression right)
    {
        throw new NotSupportedException();
    }

    public abstract bool IsAssignableTo(Type baseType);
    public abstract object Parse(string value);
    public abstract string? Stringify(object? value);
    public abstract Expression CreateBoxedConstant(object? value);
    public abstract void Accept(IDataTypeVisitor visitor);
}