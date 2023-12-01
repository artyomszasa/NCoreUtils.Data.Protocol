using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Data.Protocol;

/// <summary>
/// Represents errors that thrown when raw data query is semantically invalid.
/// </summary>
#if !NET8_0_OR_GREATER
[Serializable]
#endif
public class ProtocolTypeInferenceException : ProtocolException
{
#if !NET8_0_OR_GREATER
    protected ProtocolTypeInferenceException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
#endif

    public ProtocolTypeInferenceException(string message) : base(message) { }

    public ProtocolTypeInferenceException(string message, Exception innerException)
        : base(message, innerException)
    { }
}