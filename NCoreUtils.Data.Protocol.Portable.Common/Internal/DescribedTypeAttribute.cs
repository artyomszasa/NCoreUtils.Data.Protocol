using System;

namespace NCoreUtils.Data.Protocol.Internal;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DescribedTypeAttribute : Attribute
{
    public Type DescribedType { get; }

    public DescribedTypeAttribute(Type describedType)
        => DescribedType = describedType;
}