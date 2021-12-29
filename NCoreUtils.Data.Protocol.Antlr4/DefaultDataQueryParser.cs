using System;
using Antlr4.Runtime;
using NCoreUtils.Data.Protocol.Ast;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol;

/// <summary>
/// Default data query parser based on Antlr4 grammar.
/// </summary>
public class DefaultDataQueryParser : IDataQueryParser
{
    protected virtual ProtocolBaseVisitor<Node> CreateVisitor()
        => new Antrl4NodeVisitor();

    protected virtual ICharStream CreateInputStream(string input)
        => new AntlrInputStream(input);

    protected virtual ITokenSource CreateLexer(ICharStream charStream)
        => new ProtocolLexer(charStream);

    protected virtual ITokenStream CreateTokenStream(ITokenSource tokenSource)
        => new CommonTokenStream(tokenSource);

    protected virtual ProtocolParser CreateParser(ITokenStream tokenStream)
        => new(tokenStream);

    /// <summary>
    /// Parses input into internal AST using Antlr4 grammar.
    /// </summary>
    /// <param name="input">String that contains raw input.</param>
    /// <returns>Root node of parsed AST.</returns>
    /// <exception cref="ProtocolException">
    /// Thrown if expression is malformed.
    /// </exception>
    public virtual Node ParseQuery(string input)
    {
        try
        {
            var ctx = CreateParser(
                CreateTokenStream(
                    CreateLexer(
                        CreateInputStream(input)
                    )
                )
            ).start();
            var visitor = CreateVisitor();
            return ctx.Accept(visitor);
        }
        catch (Exception exn)
        {
            throw new ProtocolSyntaxException($"Failed to parse expression: {input}", exn);
        }
    }
}