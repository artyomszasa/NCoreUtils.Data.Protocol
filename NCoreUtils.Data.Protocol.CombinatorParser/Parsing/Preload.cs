using System.Diagnostics;
using NCoreUtils.Data.Protocol.Lexing;

namespace NCoreUtils.Data.Protocol.Parsing;

internal ref struct Preload
{
    private Lexer Lexer;

    public Token Next { get; private set; }

    public Preload(string input)
    {
        Lexer = new(input);
        Advance();
    }

    [DebuggerStepThrough]
    public void Advance()
    {
        do
        {
            Next = Lexer.Next();
        }
        while (Next.TokenType == TokenType.Ws);
    }
}