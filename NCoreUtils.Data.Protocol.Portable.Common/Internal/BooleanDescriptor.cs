using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptorAttribute(typeof(bool))]
public sealed partial class BooleanDescriptor : ITypeDescriptor
{
    private static IReadOnlyList<string> Truthy { get; } = new [] { "true", "on", "1" };

    private static IReadOnlyList<string> Falsy { get; } = new [] { "false", "off", "0" };

    public bool IsArithmetic => false;

    public bool IsEnum => false;

    public bool IsValue => true;

    public Expression CreateAndAlso(Expression self, Expression right)
        => right.Type == typeof(bool)
            ? Expression.AndAlso(self, right)
            : throw new InvalidOperationException($"Cannot create AndAlso expression from bool and {right.Type}.");

    public Expression CreateOrElse(Expression self, Expression right)
        => right.Type == typeof(bool)
            ? Expression.OrElse(self, right)
            : throw new InvalidOperationException($"Cannot create OrElse expression from bool and {right.Type}.");

    public bool IsAssignableTo(Type baseType)
        => baseType == typeof(bool);

    public object Parse(string value) => value switch
    {
        null => throw new InvalidOperationException("Unable to convert null to boolean."),
        _ when Truthy.Contains(value, StringComparer.InvariantCultureIgnoreCase) => true,
        _ when Falsy.Contains(value, StringComparer.InvariantCultureIgnoreCase) => false,
        _ => throw new InvalidOperationException($"Unable to convert \"{value}\" to boolean.")
    };

    public string Stringify(object? value) => value switch
    {
        null => throw new InvalidOperationException("Unable to convert null to boolean."),
        bool b => b ? "true" : "false",
        _ => throw new InvalidOperationException($"Unable to convert \"{value}\" to boolean.")
    };
}