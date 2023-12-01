using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Data.Protocol;

#if !NET8_0_OR_GREATER
[Serializable]
#endif
public abstract class ParserOrLexerException : Exception
{
#if !NET8_0_OR_GREATER
    protected ParserOrLexerException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { /* noop */ }
#endif

    public ParserOrLexerException(string message) : base(message) { }

    public ParserOrLexerException(string message, Exception innerException) : base(message, innerException) { }
}