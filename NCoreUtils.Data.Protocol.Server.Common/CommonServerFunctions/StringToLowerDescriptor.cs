using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.Internal;
using NCoreUtils.Data.Protocol.TypeInference;
using Names = NCoreUtils.Data.Protocol.CommonFunctionNames;

namespace NCoreUtils.Data.Protocol.CommonServerFunctions;

internal sealed class StringToLowerDescriptor : IFunctionDescriptor
{
    private static readonly ReadOnlyConstraintedTypeList _argumentTypes = new ReadOnlyConstraintedTypeListBuilder
    {
        typeof(string)
    }.Build();

    private static readonly MethodInfo _mToLower = ReflectionHelpers.GetMethod<string>("".ToLower);

    public static StringToLowerDescriptor Singleton { get; } = new();

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type ResultType => typeof(string);

    public ReadOnlyConstraintedTypeList ArgumentTypes => _argumentTypes;

    public string Name => Names.Lower;

    private StringToLowerDescriptor() { /* noop */ }

    public Expression CreateExpression(IReadOnlyList<Expression> arguments)
        => Expression.Call(arguments[0], _mToLower);
}