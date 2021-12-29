using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Data.Protocol;

/// <summary>
/// Represents errors that occur when raw data query has invalid syntax.
/// </summary>
[Serializable]
public class ProtocolSyntaxException : Exception
{
    protected ProtocolSyntaxException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }

    public ProtocolSyntaxException(string message)
        : base(message)
    { }

    public ProtocolSyntaxException(string message, Exception innerException)
        : base(message, innerException)
    { }
}