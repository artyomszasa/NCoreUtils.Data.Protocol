using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Data.Protocol;

#if !NET8_0_OR_GREATER
[Serializable]
#endif
public class ParserException : ParserOrLexerException
{
#if !NET8_0_OR_GREATER
    protected ParserException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { /* noop */ }
#endif

    public ParserException(string message) : base(message) { }

    public ParserException(string message, Exception innerException) : base(message, innerException) { }
}