using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.Internal;
using Names = NCoreUtils.Data.Protocol.CommonFunctionNames;

namespace NCoreUtils.Data.Protocol.CommonClientFunctions;

public sealed class StringToLower : IFunctionMatcher
{
    private static MethodInfo _mToLower = ReflectionHelpers.GetMethod<string>("".ToLower);

    public FunctionMatch MatchFunction(IDataUtils utils, Expression expression)
    {
        if (expression is MethodCallExpression call && call.Method.Equals(_mToLower) && call.Object is not null)
        {
            return new(Names.Lower, new Expression[] { call.Object });
        }
        return default;
    }
}