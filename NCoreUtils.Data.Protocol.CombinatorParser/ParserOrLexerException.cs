using System;

namespace NCoreUtils.Data.Protocol;

[Serializable]
public abstract class ParserOrLexerException : Exception
{
    public ParserOrLexerException(string message) : base(message) { }

    public ParserOrLexerException(string message, Exception innerException) : base(message, innerException) { }
}