using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Names = NCoreUtils.Data.Protocol.CommonFunctionNames;

namespace NCoreUtils.Data.Protocol.CommonClientFunctions;

public sealed class StringLength : IFunctionMatcher
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(string))]
    public FunctionMatch MatchFunction(IDataUtils utils, Expression expression)
    {
        if (expression is MemberExpression mexpr
            && mexpr.Expression is not null
            && mexpr.Member.DeclaringType == typeof(string)
            && mexpr.Member.Name == nameof(string.Length))
        {
            return new(Names.Length, new[] { mexpr.Expression });
        }
        return default;
    }
}