using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Data.Protocol;

[Serializable]
public abstract class ParserOrLexerException : Exception
{
    protected ParserOrLexerException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { /* noop */ }

    public ParserOrLexerException(string message) : base(message) { }

    public ParserOrLexerException(string message, Exception innerException) : base(message, innerException) { }
}