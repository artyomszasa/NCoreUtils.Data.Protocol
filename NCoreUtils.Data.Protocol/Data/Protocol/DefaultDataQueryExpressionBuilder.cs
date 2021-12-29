using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using NCoreUtils.Data.Protocol.Ast;
using NCoreUtils.Data.Protocol.Internal;
using NCoreUtils.Data.Protocol.TypeInference.Ast;

namespace NCoreUtils.Data.Protocol;

public class DefaultDataQueryExpressionBuilder : IDataQueryExpressionBuilder
{
    public IDataQueryParser Parser { get; }

    public ITypeInferrer Inferrer { get; }

    public ILogger Logger { get; }

    public DefaultDataQueryExpressionBuilder(
        IDataQueryParser parser,
        ITypeInferrer inferrer,
        ILogger<DefaultDataQueryExpressionBuilder> logger)
    {
        Parser = parser ?? throw new ArgumentNullException(nameof(parser));
        Inferrer = inferrer ?? throw new ArgumentNullException(nameof(inferrer));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates LINQ expression from the resolved internal expression.
    /// </summary>
    /// <param name="resolvedExpression">Resolved internal expression.</param>
    /// <returns>LINQ Expression representation of the resolved expression.</returns>
    protected virtual LambdaExpression CreateExpression(Lambda<Type> resolvedExpression)
        => ExpressionBuilderVisitor.Singleton.VisitLambda(resolvedExpression, Inferrer.PropertyResolver, new NameMap());

    /// <summary>
    /// Parses raw input creating internal expression.
    /// </summary>
    /// <param name="input">String that contains the expression to parse.</param>
    /// <returns>Internal expression.</returns>
    protected virtual Node ParseExpression(string input)
        => Parser.ParseQuery(input);

    /// <summary>
    /// Inters and validates types in the specified internal expression.
    /// </summary>
    /// <param name="rootType">Type of the root argument in the expression.</param>
    /// <param name="expression">Internal expression without type information.</param>
    /// <returns>Internal expression with resolved type information.</returns>
    protected virtual Lambda<Type> ResolveExpression(Type rootType, Lambda expression)
        => Inferrer.InferTypes(rootType, expression);

    /// <inheritdoc />
    public LambdaExpression BuildExpression([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type rootType, string input)
    {
        try
        {
            Logger.LogTrace("Processing expression {Expression} with root type {RootType}.", input, rootType);
            var rawExpression = ParseExpression(input);
            if (rawExpression is not Lambda rawLambda)
            {
                throw new ArgumentException($"Specified input defines non-lambda expression: {rawExpression}.");
            }
            var resolvedExpression = ResolveExpression(rootType, rawLambda);
            return CreateExpression(resolvedExpression);
        }
        catch (Exception exn)
        {
            throw new ProtocolException(
                $"Failed to build expression for \"{input}\" with root type {rootType}.",
                exn
            );
        }
    }
}