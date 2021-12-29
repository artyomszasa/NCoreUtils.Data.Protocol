using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace NCoreUtils.Data.Protocol;

/// <summary>
/// Defines functionality to parse and process raw input string into LINQ expressions.
/// </summary>
public interface IDataQueryExpressionBuilder
{
    /// <summary>
    /// Parses and processes specified query creating LINQ expression with respect to the root argument type.
    /// </summary>
    /// <param name="rootType">Type of the root argument in the expression.</param>
    /// <param name="input">Raw query to parse and process.</param>
    /// <returns>LINQ Expression representation of the input query.</returns>
    LambdaExpression BuildExpression(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type rootType,
        string input
    );
}