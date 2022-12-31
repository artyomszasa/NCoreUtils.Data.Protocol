using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

[BuiltInDescriptor(typeof(string))]
public sealed partial class StringDescriptor : ITypeDescriptor
{
    object ITypeDescriptor.Parse(string value)
        => Parse(value);

    public IReadOnlyList<PropertyInfo> Properties { get; } = new PropertyInfo[]
    {
        (PropertyInfo)((MemberExpression)((Expression<Func<string, int>>)(e => e.Length)).Body).Member
    };

    public bool IsArithmetic => false;

    public bool IsEnum => false;

    public bool IsValue => false;

    public bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(string));

    public static string Parse(string value) => value;

    public string? Stringify(object? value) => value as string;
}