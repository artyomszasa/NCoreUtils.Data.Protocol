using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.Internal;
using NCoreUtils.Data.Protocol.TypeInference;
using Names = NCoreUtils.Data.Protocol.CommonFunctionNames;

namespace NCoreUtils.Data.Protocol.CommonServerFunctions;

internal sealed class StringToUpperDescriptor : IFunctionDescriptor
{
    private static readonly ReadOnlyConstraintedTypeList _argumentTypes = new ReadOnlyConstraintedTypeListBuilder
    {
        typeof(string)
    }.Build();

    private static readonly MethodInfo _mToUpper = ReflectionHelpers.GetMethod<string>("".ToUpper);

    public static StringToUpperDescriptor Singleton { get; } = new();

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type ResultType => typeof(string);

    public ReadOnlyConstraintedTypeList ArgumentTypes => _argumentTypes;

    public string Name => Names.Upper;

    private StringToUpperDescriptor() { /* noop */ }

    public Expression CreateExpression(IReadOnlyList<Expression> arguments)
        => Expression.Call(arguments[0], _mToUpper);
}