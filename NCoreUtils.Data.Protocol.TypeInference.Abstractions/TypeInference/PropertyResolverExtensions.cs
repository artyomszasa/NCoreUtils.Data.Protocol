using System;
using System.Diagnostics.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.TypeInference;

public static class PropertyResolverExtensions
{
    public static IProperty ResolveProperty(
        this IPropertyResolver resolver,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type instanceType,
        string name)
    {
        if (resolver.TryResolveProperty(instanceType, name, out var property))
        {
            return property;
        }
        throw new ProtocolTypeInferenceException($"Type {instanceType} has no member {name}.");
    }
}