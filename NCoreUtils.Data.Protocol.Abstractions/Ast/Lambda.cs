using System;

namespace NCoreUtils.Data.Protocol.Ast;

public sealed class Lambda : Node
{
    public Identifier Arg { get; }

    public Node Body { get; }

    internal Lambda(Identifier arg, Node body)
    {
        Arg = arg ?? throw new ArgumentNullException(nameof(arg));
        Body = body ?? throw new ArgumentNullException(nameof(body));
    }

    internal override void Accept(NodeExtensions.EmplaceVisitor visitor, bool complex, ref SpanBuilder builder)
        => visitor.VisitLambda(this, complex, ref builder);

    internal override int Accept(NodeExtensions.GetStringifiedSizeVisitor visitor, bool complex)
        => visitor.VisitLambda(this, complex);

    public override TResult Accept<TArg, TResult>(INodeVisitor<TArg, TResult> visitor, TArg arg)
        => visitor.VisitLambda(this, arg);

    public override TResult Accept<TArg, TResult>(INodeRefVisitor<TArg, TResult> visitor, ref TArg arg)
        where TArg : struct
        => visitor.VisitLambda(this, ref arg);

    public override TResult Accept<TArg1, TArg2, TResult>(INodeVisitor<TArg1, TArg2, TResult> visitor, TArg1 arg1, TArg2 arg2)
        => visitor.VisitLambda(this, arg1, arg2);

    public override int GetHashCode()
        => HashCode.Combine(HashTags.Lambda, Arg, Body);
}