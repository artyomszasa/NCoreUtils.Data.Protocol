using System;

namespace NCoreUtils.Data.Protocol.Internal;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DescribedTypeAttribute(Type describedType) : Attribute
{
    public Type DescribedType { get; } = describedType;
}