using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Data.Protocol;

#if !NET8_0_OR_GREATER
[Serializable]
#endif
public class LexerException : ParserOrLexerException
{
#if !NET8_0_OR_GREATER
    protected LexerException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { /* noop */ }
#endif

    public LexerException(string message) : base(message) { }

    public LexerException(string message, Exception innerException) : base(message, innerException) { }
}