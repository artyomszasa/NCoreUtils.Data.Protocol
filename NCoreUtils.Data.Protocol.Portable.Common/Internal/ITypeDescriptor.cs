using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

public interface ITypeDescriptor
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    Type Type { get; }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    Type ArrayOfType { get; }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    Type EnumerableOfType { get; }

    IReadOnlyList<PropertyInfo> Properties { get; }

    bool IsArithmetic { get; }

    bool IsEnum { get; }

    bool IsValue { get; }

    object? BoxNullable(object value);

    bool IsAssignableTo(Type baseType);

    bool IsEnumerable([MaybeNullWhen(false)] out Type elementType);

    bool IsArray([MaybeNullWhen(false)] out Type elementType);

    bool IsLambda([MaybeNullWhen(false)] out Type argType, [MaybeNullWhen(false)] out Type resType);

    bool IsMaybe([MaybeNullWhen(false)] out Type elementType);

    bool IsNullable([MaybeNullWhen(false)] out Type elementType);

    object Parse(string value);

    string? Stringify(object? value);

    bool TryGetEnumFactory([MaybeNullWhen(false)] out IEnumFactory enumFactory);

    Expression CreateBoxedConstant(object? value);

    Expression CreateAndAlso(Expression self, Expression right);

    Expression CreateOrElse(Expression self, Expression right);

    Expression CreateEqual(Expression self, Expression right);

    Expression CreateNotEqual(Expression self, Expression right);

    Expression CreateGreaterThan(Expression self, Expression right);

    Expression CreateGreaterThanOrEqual(Expression self, Expression right);

    Expression CreateLessThan(Expression self, Expression right);

    Expression CreateLessThanOrEqual(Expression self, Expression right);

    Expression CreateAdd(Expression self, Expression right);

    Expression CreateSubtract(Expression self, Expression right);

    Expression CreateMultiply(Expression self, Expression right);

    Expression CreateDivide(Expression self, Expression right);

    Expression CreateModulo(Expression self, Expression right);

    MethodInfo EnumerableAnyMethod { get; }

    MethodInfo EnumerableAllMethod { get; }

    MethodInfo EnumerableContainsMethod { get; }

    void Accept(IDataTypeVisitor visitor);
}