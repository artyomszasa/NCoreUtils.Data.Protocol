using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol.Unit;

public sealed class SomeComplexData(Stream stream) : IDisposable
{
    private readonly object _sync = new();

    private int _isDisposed;

    private string? _computedValue;

    public Stream Stream { get; } = stream;

    public string ComputedValue
    {
        get
        {
            if (_computedValue is null)
            {
                lock(_sync)
                {
                    if (_computedValue is null)
                    {
                        using var reader = new StreamReader(Stream);
                        _computedValue = reader.ReadToEnd();
                    }
                }
            }
            return _computedValue;
        }
    }

    public void Dispose()
    {
        if (0 == Interlocked.CompareExchange(ref _isDisposed, 1, 0))
        {
            Stream.Dispose();
        }
    }
}

public sealed class ExplicitComplexDataDescriptor : ITypeDescriptor<SomeComplexData>
{
    public sealed class Box(SomeComplexData? value)
    {
        public readonly SomeComplexData? Value = value;

        public override string ToString() => $"{{{Value}}}";
    }

    private static FieldInfo BoxValueField { get; } = (FieldInfo)((MemberExpression)((Expression<Func<Box, SomeComplexData>>)(e => e.Value!)).Body).Member;

    private static readonly IReadOnlyList<PropertyInfo> _properties =
    [
        (PropertyInfo)((MemberExpression)((Expression<Func<SomeComplexData, string>>)(e => e.ComputedValue)).Body).Member
    ];

    private static MethodInfo GetMethod<TArg1, TArg2, TResult>(Func<TArg1, TArg2, TResult> func)
        => func.Method;

    public Type ArrayOfType => typeof(SomeComplexData[]);

    public Type EnumerableOfType => typeof(IEnumerable<SomeComplexData>);

    public IReadOnlyList<PropertyInfo> Properties => _properties;

    public bool IsArithmetic => false;

    public bool IsEnum => false;

    public bool IsValue => false;

    public MethodInfo EnumerableAnyMethod => GetMethod<IEnumerable<SomeComplexData>, Func<SomeComplexData, bool>, bool>(Enumerable.Any);

    public MethodInfo EnumerableAllMethod => GetMethod<IEnumerable<SomeComplexData>, Func<SomeComplexData, bool>, bool>(Enumerable.All);

    public MethodInfo EnumerableContainsMethod => GetMethod<IEnumerable<SomeComplexData>, SomeComplexData, bool>(Enumerable.Contains);

    public void Accept(IDataTypeVisitor visitor)
        => visitor.Visit<SomeComplexData>();

    public object? BoxNullable(object value) => throw new InvalidOperationException("Unable to create nullable from reference type.");

    public Expression CreateAdd(Expression self, Expression right) => throw new NotSupportedException();

    public Expression CreateAndAlso(Expression self, Expression right) => throw new NotSupportedException();

    public Expression CreateBoxedConstant(object? value)
        => Expression.Field(Expression.Constant(new Box((SomeComplexData?)value)), BoxValueField);

    public Expression CreateDivide(Expression self, Expression right) => throw new NotSupportedException();

    public Expression CreateEqual(Expression self, Expression right) => throw new NotSupportedException();

    public Expression CreateGreaterThan(Expression self, Expression right) => throw new NotSupportedException();

    public Expression CreateGreaterThanOrEqual(Expression self, Expression right) => throw new NotSupportedException();

    public Expression CreateLessThan(Expression self, Expression right) => throw new NotSupportedException();

    public Expression CreateLessThanOrEqual(Expression self, Expression right) => throw new NotSupportedException();

    public Expression CreateModulo(Expression self, Expression right) => throw new NotSupportedException();

    public Expression CreateMultiply(Expression self, Expression right) => throw new NotSupportedException();

    public Expression CreateNotEqual(Expression self, Expression right) => throw new NotSupportedException();

    public Expression CreateOrElse(Expression self, Expression right) => throw new NotSupportedException();

    public Expression CreateSubtract(Expression self, Expression right) => throw new NotSupportedException();

    public bool IsArray([MaybeNullWhen(false)] out Type elementType)
    {
        elementType = default;
        return false;
    }

    public bool IsAssignableTo(Type baseType)
        => baseType == typeof(SomeComplexData);

    public bool IsEnumerable([MaybeNullWhen(false)] out Type elementType)
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

    public object Parse(string value) => throw new NotSupportedException();

    public string? Stringify(object? value) => throw new NotSupportedException();

    public bool TryGetEnumFactory([MaybeNullWhen(false)] out IEnumFactory enumFactory)
    {
        enumFactory = default;
        return false;
    }
}