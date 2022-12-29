using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol;

public partial class ReflectionDataUtils
{
    private static MethodInfo? _gAny;

    private static MethodInfo? _gAll;

    public MethodInfo GetEnumerableAnyMethod(Type type)
    {
        _gAny ??= ReflectionHelpers.GetMethod<IEnumerable<int>, Func<int, bool>, bool>(Enumerable.Any).GetGenericMethodDefinition();
        return _gAny.MakeGenericMethod(type);
    }

    public MethodInfo GetEnumerableAllMethod(Type type)
    {
        _gAll ??= ReflectionHelpers.GetMethod<IEnumerable<int>, Func<int, bool>, bool>(Enumerable.All).GetGenericMethodDefinition();
        return _gAll.MakeGenericMethod(type);
    }
}