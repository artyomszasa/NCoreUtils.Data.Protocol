using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.Internal;
using Names = NCoreUtils.Data.Protocol.CommonFunctionNames;

namespace NCoreUtils.Data.Protocol.CommonClientFunctions;

public sealed class StringToUpper : IFunctionMatcher
{
    private static readonly MethodInfo _mToLower = ReflectionHelpers.GetMethod<string>("".ToUpper);

    public FunctionMatch MatchFunction(IDataUtils utils, Expression expression)
    {
        if (expression is MethodCallExpression call && call.Method.Equals(_mToLower) && call.Object is not null)
        {
            return new(Names.Upper, new Expression[] { call.Object });
        }
        return default;
    }
}