using System;

namespace NCoreUtils.Data.Protocol.Ast;

public sealed class Constant : Node
{
    public string? RawValue { get; }

    internal Constant(string? rawValue)
        => RawValue = rawValue;

    internal override void Accept(NodeExtensions.EmplaceVisitor visitor, bool complex, ref SpanBuilder builder)
        => visitor.VisitConstant(this, complex, ref builder);

    internal override int Accept(NodeExtensions.GetStringifiedSizeVisitor visitor, bool complex)
        => visitor.VisitConstant(this, complex);

    public override TResult Accept<TArg, TResult>(INodeVisitor<TArg, TResult> visitor, TArg arg)
        => visitor.VisitConstant(this, arg);

    public override TResult Accept<TArg, TResult>(INodeRefVisitor<TArg, TResult> visitor, ref TArg arg)
        where TArg : struct
        => visitor.VisitConstant(this, ref arg);

    public override TResult Accept<TArg1, TArg2, TResult>(INodeRefVisitor<TArg1, TArg2, TResult> visitor, ref TArg1 arg1, TArg2 arg2)
        where TArg1 : struct
        => visitor.VisitConstant(this, ref arg1, arg2);

    public override TResult Accept<TArg1, TArg2, TResult>(INodeVisitor<TArg1, TArg2, TResult> visitor, TArg1 arg1, TArg2 arg2)
        => visitor.VisitConstant(this, arg1, arg2);

    public override int GetHashCode()
        => HashCode.Combine(HashTags.Constant, RawValue?.GetHashCode() ?? 0);
}