using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.TypeInference;

public class DefaultProperty : IProperty
{
    public PropertyInfo Property { get; }

    public Type PropertyType
    {
        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        [UnconditionalSuppressMessage("Trimming", "IL2073", Justification = "Property type required to be preserved in constructor.")]
        get => Property.PropertyType;
    }

    [RequiresUnreferencedCode("Declaring type must be preserved.")]
    public DefaultProperty(PropertyInfo property)
        => Property = property ?? throw new ArgumentNullException(nameof(property));

    public Expression CreateExpression(Expression instance)
        => Expression.Property(instance, Property);
}