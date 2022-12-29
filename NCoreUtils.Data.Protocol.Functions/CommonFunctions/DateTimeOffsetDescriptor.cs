using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#if !NET6_0_OR_GREATER
using System.Linq;
#endif
using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public sealed class DateTimeOffsetDescriptor : IFunctionDescriptor
{
    private static readonly ReadOnlyConstraintedTypeList _argumentTypes = new ReadOnlyConstraintedTypeListBuilder()
        .Add(typeof(long))
        .Build();

    // FIXME
    private static readonly ConstructorInfo _dateTimeOffsetCtor = typeof(DateTimeOffset)
#if NET6_0_OR_GREATER
        .GetConstructor(BindingFlags.Instance | BindingFlags.Public, new [] { typeof(long), typeof(TimeSpan) })
#else
        .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
        .FirstOrDefault(c =>
        {
            var parameters = c.GetParameters();
            return parameters.Length == 2
                && parameters[0].ParameterType == typeof(long)
                && parameters[1].ParameterType == typeof(TimeSpan);
        })
#endif
        ?? throw new InvalidOperationException("Unable to get DateTimeOffset constructor.");

    private static ConstantExpression TimeSpanZeroArg { get; } = Expression.Constant(TimeSpan.Zero, typeof(TimeSpan));

    public static DateTimeOffsetDescriptor Singleton { get; } = new DateTimeOffsetDescriptor();

    public Type ResultType
    {
        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        get => typeof(DateTimeOffset);
    }

    public ReadOnlyConstraintedTypeList ArgumentTypes => _argumentTypes;

    public string Name => Names.DateTimeOffset;

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DateTimeOffset))]
    private DateTimeOffsetDescriptor() { }

    public Expression CreateExpression(IReadOnlyList<Expression> arguments)
        => Expression.New(_dateTimeOffsetCtor, arguments[0], TimeSpanZeroArg);
}