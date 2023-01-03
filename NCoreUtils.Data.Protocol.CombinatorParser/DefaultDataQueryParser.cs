using NCoreUtils.Data.Protocol.Ast;

namespace NCoreUtils.Data.Protocol;

public class DefaultDataQueryParser : IDataQueryParser
{
    public Node ParseQuery(string input)
    {
        var parser = new Parsing.Parser(input);
        return parser.Start();
    }
}