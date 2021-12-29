using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Data.Protocol;

/// <summary>
/// Represents errors that occur during protocol related operations.
/// </summary>
[Serializable]
public class ProtocolException : Exception
{
    protected ProtocolException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }

    public ProtocolException(string message)
        : base(message)
    { }

    public ProtocolException(string message, Exception innerException)
        : base(message, innerException)
    { }
}