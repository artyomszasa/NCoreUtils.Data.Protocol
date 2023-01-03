using System.Linq.Expressions;

namespace NCoreUtils.Data.Protocol;

/// <summary>
/// Defines functionality to match protocol function invocations.
/// </summary>
public interface IFunctionMatcher
{
    /// <summary>
    /// Matches expression against supported functions.
    /// </summary>
    /// <param name="utils">Utility helper.</param>
    /// <param name="expression">Expression to match.</param>
    /// <returns>Function matching result.</returns>
    FunctionMatch MatchFunction(IDataUtils utils, Expression expression);
}