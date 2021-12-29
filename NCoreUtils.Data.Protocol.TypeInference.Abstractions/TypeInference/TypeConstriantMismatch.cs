using System;

namespace NCoreUtils.Data.Protocol.TypeInference;

[Serializable]
public struct TypeConstriantMismatch
{
    public TypeRef TargetType { get; }

    public TypeConstriantMismatchReason Reason { get; }

    public TypeConstriantMismatch(TypeRef targetType, TypeConstriantMismatchReason reason)
    {
        TargetType = targetType;
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
    }

    public override string ToString()
        => Reason.ToString(TargetType);
}