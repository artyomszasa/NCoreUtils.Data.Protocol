namespace NCoreUtils.Data.Protocol.Generator;

public partial class ProtocolContextGeneratorV2
{
    private const string attributeSource = @"#nullable enable
using System;

namespace NCoreUtils.Data.Protocol
{
    [Flags]
    internal enum ProtocolGenerationMode
    {
        Minimal = 0x00,
        Predicates = 0x01,
        Array = 0x02,
        Enumerable = 0x04,
        Nullable = 0x08,
        Optimal = Predicates | Enumerable,
        Full = Predicates | Array | Enumerable | Nullable
    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    internal sealed class ProtocolEntityAttribute : Attribute
    {
        public Type EntityType { get; }

        public ProtocolEntityAttribute(Type entityType)
        {
            EntityType = entityType;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    internal sealed class ProtocolLambdaAttribute : Attribute
    {
        public Type ArgType { get; }

        public Type ResType { get; }

        public ProtocolLambdaAttribute(Type argType, Type resType)
        {
            ArgType = argType;
            ResType = resType;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    internal sealed class ProtocolGenerationOptionsAttribute : Attribute
    {
        public ProtocolGenerationMode Mode { get; }

        public ProtocolGenerationOptionsAttribute(ProtocolGenerationMode mode)
        {
            Mode = mode;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    internal sealed class ProtocolDescriptorAttribute : Attribute
    {
        public Type DescriptorType { get; }

        public ProtocolDescriptorAttribute(Type descriptorType)
            => DescriptorType = descriptorType;
    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    internal sealed class ProtocolOpaqueAttribute : Attribute
    {
        public Type Type { get; }

        public ProtocolOpaqueAttribute(Type type)
            => Type = type;
    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    internal sealed class ProtocolSafeNameAttribute : Attribute
    {
        public Type Type { get; }

        public string Name { get; }

        public ProtocolSafeNameAttribute(Type type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}";
}