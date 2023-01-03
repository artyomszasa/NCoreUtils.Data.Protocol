using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Data.Protocol;

/// <summary>
/// Represents errors that thrown when raw data query is semantically invalid.
/// </summary>
[Serializable]
public class ProtocolTypeInferenceException : ProtocolException
{
    protected ProtocolTypeInferenceException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }

    public ProtocolTypeInferenceException(string message) : base(message) { }

    public ProtocolTypeInferenceException(string message, Exception innerException)
        : base(message, innerException)
    { }
}