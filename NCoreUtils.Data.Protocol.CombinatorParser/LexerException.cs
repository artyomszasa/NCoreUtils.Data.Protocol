using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Data.Protocol;

[Serializable]
public class LexerException : ParserOrLexerException
{
    protected LexerException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { /* noop */ }

    public LexerException(string message) : base(message) { }

    public LexerException(string message, Exception innerException) : base(message, innerException) { }
}