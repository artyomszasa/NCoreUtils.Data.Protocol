using System;

namespace NCoreUtils.Data.Protocol.TypeInference;

/// <summary>
/// Represents type constraint mismatch detials.
/// </summary>
[Serializable]
public abstract class TypeConstriantMismatchReason
{
    /// <summary>
    /// Examined type is missing some constrainted member.
    /// </summary>
    [Serializable]
    public sealed class MissingMember(string memberName) : TypeConstriantMismatchReason
    {
        public string MemberName { get; } = memberName;

        public override string ToString(TypeRef type)
            => $"Type {type} has no member {MemberName}.";
    }

    /// <summary>
    /// Examined type is missing some constrainted interface.
    /// </summary>
    [Serializable]
    public sealed class MissingInterfaceImplmentation(TypeRef @interface) : TypeConstriantMismatchReason
    {
        public TypeRef Interface { get; } = @interface;

        public override string ToString(TypeRef type)
            => $"Type {type} does not implement {Interface}.";
    }

    /// <summary>
    /// Examined type is incompatible with the constainted base type.
    /// </summary>
    [Serializable]
    public sealed class IncompatibleType(TypeRef baseType) : TypeConstriantMismatchReason
    {
        public TypeRef BaseType { get; } = baseType;

        public override string ToString(TypeRef type)
            => $"Type {type} is not compatible with constrainted base type {BaseType}.";
    }

    /// <summary>
    /// Examined type is not numeric but constrainted to be so.
    /// </summary>
    [Serializable]
    public sealed class NumericConstraint : TypeConstriantMismatchReason
    {
        public static NumericConstraint Instance { get; } = new NumericConstraint();

        private NumericConstraint() { }

        public override string ToString(TypeRef type)
            => $"Type {type} has been constrainted to be numeric";
    }

    /// <summary>
    /// Examined type is numeric but constrainted not to be so.
    /// </summary>
    [Serializable]
    public sealed class NonNumericConstraint : TypeConstriantMismatchReason
    {
        public static NonNumericConstraint Instance { get; } = new NonNumericConstraint();

        private NonNumericConstraint() { }

        public override string ToString(TypeRef type)
            => $"Type {type} has been constrainted to be non-numeric.";
    }

    /// <summary>
    /// Examined type is not nullable but constrainted to be so.
    /// </summary>
    [Serializable]
    public sealed class NullableConstraint : TypeConstriantMismatchReason
    {
        public static NullableConstraint Instance { get; } = new NullableConstraint();

        private NullableConstraint() { }

        public override string ToString(TypeRef type)
            => $"Type {type} has been constrainted to be nullable.";
    }

    /// <summary>
    /// Examined type is nullable but constrainted not to be so.
    /// </summary>
    [Serializable]
    public sealed class NonNullableConstraint : TypeConstriantMismatchReason
    {
        public static NonNullableConstraint Instance { get; } = new NonNullableConstraint();

        private NonNullableConstraint() { }

        public override string ToString(TypeRef type)
            => $"Type {type} has been constrainted to be non-nullable.";
    }

    /// <summary>
    /// Examined type is not a lambda but constrainted to be so.
    /// </summary>
    [Serializable]
    public sealed class LambdaConstraint : TypeConstriantMismatchReason
    {
        public static LambdaConstraint Instance { get; } = new LambdaConstraint();

        private LambdaConstraint() { }

        public override string ToString(TypeRef type)
            => $"Type {type} has been constrainted to be lambda.";
    }

    private TypeConstriantMismatchReason() { }

    /// <summary>
    /// Provides user friendly message about the mismatch.
    /// </summary>
    /// <param name="type">Related type.</param>
    public abstract string ToString(TypeRef type);
}