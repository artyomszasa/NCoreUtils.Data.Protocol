using System;
using System.Reflection;

namespace NCoreUtils.Data.Protocol;

public partial interface IDataUtils
{
    MethodInfo GetEnumerableAnyMethod(Type type);

    MethodInfo GetEnumerableAllMethod(Type type);
}