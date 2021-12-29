using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.Internal;

public sealed class PropertyAsFunctionDescriptor : IFunctionDescriptor
{
    public PropertyInfo Property { get; }

    public Type ResultType => Property.PropertyType;

    public ReadOnlyConstraintedTypeList ArgumentTypes { get; }

    public string Name { get; }

    [RequiresUnreferencedCode("ArgumentType must be preserved event if it is explicitly defined through property parameter.")]
    public PropertyAsFunctionDescriptor(PropertyInfo property, Type? argumentType, string name)
    {
        Property = property;
        ArgumentTypes = new ReadOnlyConstraintedTypeListBuilder
        {
            argumentType ?? property.DeclaringType ?? throw new InvalidOperationException("Argument type cannot be deducted and must be specified explicitly.")
        }.Build();
        Name = name;
    }

    public Expression CreateExpression(IReadOnlyList<Expression> arguments)
        => Expression.Property(arguments[0], Property);
}