using System;
using System.Reflection;

namespace NCoreUtils.Data.Protocol;

public partial class PortableDataUtils
{
    public MethodInfo GetEnumerableAnyMethod(Type type)
        => GetDescriptor(type).EnumerableAnyMethod;

    public MethodInfo GetEnumerableAllMethod(Type type)
        => GetDescriptor(type).EnumerableAllMethod;

    public MethodInfo GetEnumerableContainsMethod(Type type)
        => GetDescriptor(type).EnumerableContainsMethod;
}