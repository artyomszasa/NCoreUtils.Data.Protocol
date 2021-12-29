using System;
using System.Diagnostics.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.TypeInference;

public interface IPropertyResolver
{
    bool TryResolveProperty(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type instanceType,
        string propertyName,
        [MaybeNullWhen(false)] out IProperty property);
}