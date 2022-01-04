using System;

namespace NCoreUtils.Data.Protocol.Ast;

public sealed class Binary : Node
{
    public Node Left { get; }

    public BinaryOperation Operation { get; }

    public Node Right { get; }

    internal Binary(Node left, BinaryOperation operation, Node right)
    {
        Left = left ?? throw new ArgumentNullException(nameof(left));
        Operation = operation;
        Right = right ?? throw new ArgumentNullException(nameof(right));
    }

    internal override void Accept(NodeExtensions.EmplaceVisitor visitor, bool complex, ref SpanBuilder builder)
        => visitor.VisitBinary(this, complex, ref builder);

    internal override int Accept(NodeExtensions.GetStringifiedSizeVisitor visitor, bool complex)
        => visitor.VisitBinary(this, complex);

    public override TResult Accept<TArg, TResult>(INodeVisitor<TArg, TResult> visitor, TArg arg)
        => visitor.VisitBinary(this, arg);

    public override TResult Accept<TArg, TResult>(INodeRefVisitor<TArg, TResult> visitor, ref TArg arg)
        where TArg : struct
        => visitor.VisitBinary(this, ref arg);

    public override TResult Accept<TArg1, TArg2, TResult>(INodeRefVisitor<TArg1, TArg2, TResult> visitor, ref TArg1 arg1, TArg2 arg2)
        where TArg1 : struct
        => visitor.VisitBinary(this, ref arg1, arg2);

    public override TResult Accept<TArg1, TArg2, TResult>(INodeVisitor<TArg1, TArg2, TResult> visitor, TArg1 arg1, TArg2 arg2)
        => visitor.VisitBinary(this, arg1, arg2);

    // public override int GetHashCode()
    //     => HashCode.Combine(HashTags.Binary, Left, Operation, Right);
}