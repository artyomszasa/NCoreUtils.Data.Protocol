using System;

namespace NCoreUtils.Data.Protocol.TypeInference;

[Serializable]
public readonly struct TypeConstriantMismatch(TypeRef targetType, TypeConstriantMismatchReason reason)
{
    public TypeRef TargetType { get; } = targetType;

    public TypeConstriantMismatchReason Reason { get; } = reason ?? throw new ArgumentNullException(nameof(reason));

    public override string ToString()
        => Reason.ToString(TargetType);
}