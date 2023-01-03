using System;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

public static class ReflectionHelpers
{
    public static MethodInfo GetMethod<TResult>(Func<TResult> func)
        => func.Method;

    public static MethodInfo GetMethod<TArg, TResult>(Func<TArg, TResult> func)
        => func.Method;

    public static MethodInfo GetMethod<TArg1, TArg2, TResult>(Func<TArg1, TArg2, TResult> func)
        => func.Method;
}