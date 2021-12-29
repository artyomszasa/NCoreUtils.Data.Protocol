using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace NCoreUtils.Data.Protocol.Internal;

public partial class ExpressionBuilderVisitor
{
    private static Dictionary<Type, Func<string, Expression>> CommonConstantVisitors { get; } = new()
    {
        { typeof(string), VisitStringConstant },
        { typeof(int), VisitInt32Constant },
        { typeof(bool), VisitBooleanConstant }
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Expression VisitStringConstant(string raw)
        => BoxedConstantBuilder<string>.BuildExpression(raw);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Expression VisitInt32Constant(string raw)
    {
        var i32 = int.Parse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture);
        return BoxedConstantBuilder<int>.BuildExpression(i32);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Expression VisitBooleanConstant(string raw)
    {
        var boolean = bool.Parse(raw);
        return BoxedConstantBuilder<bool>.BuildExpression(boolean);
    }
}