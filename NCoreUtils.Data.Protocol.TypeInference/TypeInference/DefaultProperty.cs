using System;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.TypeInference;

internal class DefaultProperty(PropertyInfo property) : IProperty
{
    public PropertyInfo Property { get; } = property ?? throw new ArgumentNullException(nameof(property));

    public Type PropertyType => Property.PropertyType;

    public Expression CreateExpression(Expression instance)
        => Expression.Property(instance, Property);
}