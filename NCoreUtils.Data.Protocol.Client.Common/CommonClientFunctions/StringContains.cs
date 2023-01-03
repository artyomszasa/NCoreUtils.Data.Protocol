using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.Internal;
using Names = NCoreUtils.Data.Protocol.CommonFunctionNames;

namespace NCoreUtils.Data.Protocol.CommonClientFunctions;

public sealed class StringContains : IFunctionMatcher
{
    private static MethodInfo _mContains = ReflectionHelpers.GetMethod<string, bool>("".Contains);

    public FunctionMatch MatchFunction(IDataUtils utils, Expression expression)
    {
        if (expression is MethodCallExpression call && call.Method.Equals(_mContains) && call.Object is not null)
        {
            return new(Names.Contains, new Expression[]
            {
                call.Object,
                call.Arguments[0]
            });
        }
        return default;
    }
}