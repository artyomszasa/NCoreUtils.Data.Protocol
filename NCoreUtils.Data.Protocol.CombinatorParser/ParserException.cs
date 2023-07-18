using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Data.Protocol;

[Serializable]
public class ParserException : ParserOrLexerException
{
    protected ParserException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { /* noop */ }

    public ParserException(string message) : base(message) { }

    public ParserException(string message, Exception innerException) : base(message, innerException) { }
}