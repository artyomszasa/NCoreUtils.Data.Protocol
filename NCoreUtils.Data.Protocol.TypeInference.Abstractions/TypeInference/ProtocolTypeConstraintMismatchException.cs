using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Data.Protocol.TypeInference;

public class ProtocolTypeConstraintMismatchException : ProtocolTypeInferenceException
{
    public TypeConstriantMismatch Details { get; }

    protected ProtocolTypeConstraintMismatchException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        => Details = (TypeConstriantMismatch)info.GetValue(nameof(Details), typeof(TypeConstriantMismatch))!;

    public ProtocolTypeConstraintMismatchException(TypeConstriantMismatch details, string? message = default)
        : base(message ?? details.ToString())
        => Details = details;

    public ProtocolTypeConstraintMismatchException(TypeConstriantMismatch details, string? message, Exception innerException)
        : base(message ?? details.ToString(), innerException)
        => Details = details;

    public ProtocolTypeConstraintMismatchException(TypeConstriantMismatch details, Exception innerException)
        : this(details, default, innerException)
    { }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(Details), Details, typeof(TypeConstriantMismatch));
    }
}