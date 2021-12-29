using NCoreUtils.Data.Protocol.Ast;

namespace NCoreUtils.Data.Protocol;

/// <summary>
/// Defines functionality to parse raw input string into internal AST.
/// </summary>
public interface IDataQueryParser
{
    /// <summary>
    /// Parses input into internal AST.
    /// </summary>
    /// <param name="input">String that contains raw input.</param>
    /// <returns>Root node of parsed AST.</returns>
    /// <exception cref="ProtocolException">
    /// Thrown if expression is malformed.
    /// </exception>
    Node ParseQuery(string input);
}