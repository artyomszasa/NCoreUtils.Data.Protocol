using System;

namespace NCoreUtils.Data.Protocol;

[Serializable]
public class ParserException : ParserOrLexerException
{
    public ParserException(string message) : base(message) { }

    public ParserException(string message, Exception innerException) : base(message, innerException) { }
}