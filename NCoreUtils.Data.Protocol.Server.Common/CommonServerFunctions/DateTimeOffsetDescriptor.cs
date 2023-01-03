using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.TypeInference;
using Names = NCoreUtils.Data.Protocol.CommonFunctionNames;

namespace NCoreUtils.Data.Protocol.CommonServerFunctions;

public sealed class DateTimeOffsetDescriptor : IFunctionDescriptor
{
    private static readonly ReadOnlyConstraintedTypeList _argumentTypes = new ReadOnlyConstraintedTypeListBuilder
    {
        typeof(long)
    }.Build();

    private static readonly ConstructorInfo _dateTimeOffsetCtor;

    private static ConstantExpression TimeSpanZeroArg { get; } = Expression.Constant(TimeSpan.Zero, typeof(TimeSpan));

    public static DateTimeOffsetDescriptor Singleton { get; } = new DateTimeOffsetDescriptor();

    static DateTimeOffsetDescriptor()
    {
        _dateTimeOffsetCtor = ((NewExpression)((Expression<Func<DateTimeOffset>>)(() => new DateTimeOffset(default(long), default(TimeSpan)))).Body).Constructor
            ?? throw new InvalidOperationException("Unable to get DateTimeOffset constructor.");
    }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type ResultType => typeof(DateTimeOffset);

    public ReadOnlyConstraintedTypeList ArgumentTypes => _argumentTypes;

    public string Name => Names.DateTimeOffset;

    private DateTimeOffsetDescriptor() { }

    public Expression CreateExpression(IReadOnlyList<Expression> arguments)
        => Expression.New(_dateTimeOffsetCtor, arguments[0], TimeSpanZeroArg);
}