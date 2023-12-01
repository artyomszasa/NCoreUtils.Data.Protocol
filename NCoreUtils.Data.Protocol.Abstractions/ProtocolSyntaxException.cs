using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Data.Protocol;

/// <summary>
/// Represents errors that occur when raw data query has invalid syntax.
/// </summary>
#if !NET8_0_OR_GREATER
[Serializable]
#endif
public class ProtocolSyntaxException : Exception
{
#if !NET8_0_OR_GREATER
    protected ProtocolSyntaxException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
#endif

    public ProtocolSyntaxException(string message)
        : base(message)
    { }

    public ProtocolSyntaxException(string message, Exception innerException)
        : base(message, innerException)
    { }
}