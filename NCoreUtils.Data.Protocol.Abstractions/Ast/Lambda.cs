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

    public override TResult Accept<TArg1, TArg2, TResult>(INodeRefVisitor<TArg1, TArg2, TResult> visitor, ref TArg1 arg1, TArg2 arg2)
        where TArg1 : struct
        => visitor.VisitLambda(this, ref arg1, arg2);

    public override TResult Accept<TArg1, TArg2, TResult>(INodeVisitor<TArg1, TArg2, TResult> visitor, TArg1 arg1, TArg2 arg2)
        => visitor.VisitLambda(this, arg1, arg2);
}