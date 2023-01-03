using System;

namespace NCoreUtils.Data.Protocol;

[Serializable]
public class LexerException : ParserOrLexerException
{
    public LexerException(string message) : base(message) { }

    public LexerException(string message, Exception innerException) : base(message, innerException) { }
}