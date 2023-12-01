using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Data.Protocol;

/// <summary>
/// Represents errors that occur during protocol related operations.
/// </summary>
#if !NET8_0_OR_GREATER
[Serializable]
#endif
public class ProtocolException : Exception
{
#if !NET8_0_OR_GREATER
    protected ProtocolException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
#endif

    public ProtocolException(string message)
        : base(message)
    { }

    public ProtocolException(string message, Exception innerException)
        : base(message, innerException)
    { }
}