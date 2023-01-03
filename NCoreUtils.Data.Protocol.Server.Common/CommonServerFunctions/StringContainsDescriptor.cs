using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.Internal;
using NCoreUtils.Data.Protocol.TypeInference;
using Names = NCoreUtils.Data.Protocol.CommonFunctionNames;

namespace NCoreUtils.Data.Protocol.CommonServerFunctions;

internal sealed class StringContainsDescriptor : IFunctionDescriptor
{
    private static readonly ReadOnlyConstraintedTypeList _argumentTypes = new ReadOnlyConstraintedTypeListBuilder
    {
        typeof(string),
        typeof(string)
    }.Build();

    private static readonly MethodInfo _mContains = ReflectionHelpers.GetMethod<string, bool>("".Contains);

    public static StringContainsDescriptor Singleton { get; } = new();

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type ResultType => typeof(bool);

    public ReadOnlyConstraintedTypeList ArgumentTypes => _argumentTypes;

    public string Name => Names.Contains;

    private StringContainsDescriptor() { /* noop */ }

    public Expression CreateExpression(IReadOnlyList<Expression> arguments)
        => Expression.Call(arguments[0], _mContains, arguments[1]);
}