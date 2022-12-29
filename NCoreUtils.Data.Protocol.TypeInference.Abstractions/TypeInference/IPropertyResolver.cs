using System;
using System.Diagnostics.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.TypeInference;

public interface IPropertyResolver
{
    bool TryResolveProperty(
        Type instanceType,
        string propertyName,
        [MaybeNullWhen(false)] out IProperty property);
}