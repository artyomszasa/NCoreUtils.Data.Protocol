using Microsoft.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.Generator;

public readonly struct ExplicitDescriptor
{
    public ITypeSymbol DescriptorType { get; }

    public ITypeSymbol TargetType { get; }

    public ExplicitDescriptor(ITypeSymbol descriptorType, ITypeSymbol targetType)
    {
        DescriptorType = descriptorType ?? throw new System.ArgumentNullException(nameof(descriptorType));
        TargetType = targetType ?? throw new System.ArgumentNullException(nameof(targetType));
    }
}