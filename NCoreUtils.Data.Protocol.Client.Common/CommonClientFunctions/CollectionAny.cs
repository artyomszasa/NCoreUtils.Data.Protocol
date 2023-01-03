using System.Linq;
using System.Linq.Expressions;
using Names = NCoreUtils.Data.Protocol.CommonFunctionNames;

namespace NCoreUtils.Data.Protocol.CommonClientFunctions;

public sealed class CollectionAny : IFunctionMatcher
{
    public FunctionMatch MatchFunction(IDataUtils utils, Expression expression)
    {
        if (expression is MethodCallExpression call)
        {
            var m = call.Method;
            if (m.DeclaringType == typeof(Enumerable)
                && m.Name == nameof(Enumerable.Any)
                && m.GetParameters().Length == 2)
            {
                return new(Names.Some, call.Arguments);
            }
        }
        return default;
    }
}