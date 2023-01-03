using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.TypeInference;
using Names = NCoreUtils.Data.Protocol.CommonFunctionNames;

namespace NCoreUtils.Data.Protocol.CommonServerFunctions;

internal sealed class StringLengthDescriptor : IFunctionDescriptor
{
    private static readonly ReadOnlyConstraintedTypeList _argumentTypes = new ReadOnlyConstraintedTypeListBuilder
    {
        typeof(string)
    }.Build();

    private static readonly PropertyInfo _mLength
        = (PropertyInfo)((MemberExpression)((Expression<Func<string, int>>)(s => s.Length)).Body).Member;

    public static StringLengthDescriptor Singleton { get; } = new();

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type ResultType => typeof(int);

    public ReadOnlyConstraintedTypeList ArgumentTypes => _argumentTypes;

    public string Name => Names.Length;

    private StringLengthDescriptor() { /* noop */ }

    public Expression CreateExpression(IReadOnlyList<Expression> arguments)
        => Expression.Property(arguments[0], _mLength);
}