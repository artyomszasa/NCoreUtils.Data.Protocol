using System;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.TypeInference;

internal class DefaultProperty : IProperty
{
    public PropertyInfo Property { get; }

    public Type PropertyType => Property.PropertyType;

    public DefaultProperty(PropertyInfo property)
        => Property = property ?? throw new ArgumentNullException(nameof(property));

    public Expression CreateExpression(Expression instance)
        => Expression.Property(instance, Property);
}