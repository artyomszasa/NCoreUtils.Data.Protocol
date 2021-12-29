using System;

namespace NCoreUtils.Data.Protocol.Ast;

public sealed class Identifier : Node
{
    public UniqueString Value { get; }

    internal Identifier(UniqueString value)
        => Value = value ?? throw new ArgumentNullException(nameof(value));

    internal override void Accept(NodeExtensions.EmplaceVisitor visitor, bool complex, ref SpanBuilder builder)
        => visitor.VisitIdentifier(this, complex, ref builder);

    internal override int Accept(NodeExtensions.GetStringifiedSizeVisitor visitor, bool complex)
        => visitor.VisitIdentifier(this, complex);

    public override TResult Accept<TArg, TResult>(INodeVisitor<TArg, TResult> visitor, TArg arg)
        => visitor.VisitIdentifier(this, arg);

    public override TResult Accept<TArg, TResult>(INodeRefVisitor<TArg, TResult> visitor, ref TArg arg)
        where TArg : struct
        => visitor.VisitIdentifier(this, ref arg);

    public override TResult Accept<TArg1, TArg2, TResult>(INodeVisitor<TArg1, TArg2, TResult> visitor, TArg1 arg1, TArg2 arg2)
        => visitor.VisitIdentifier(this, arg1, arg2);

    public override int GetHashCode()
        => HashCode.Combine(HashTags.Identifier, Value.GetHashCode());
}