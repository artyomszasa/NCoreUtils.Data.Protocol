using System;
using System.Collections.Immutable;
using System.Diagnostics;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol.Ast;

public sealed class Constant : Node
{
    public string? RawValue { get; }

    [DebuggerStepThrough]
    internal Constant(string? rawValue)
        => RawValue = rawValue;

    internal override void Accept(NodeExtensions.EmplaceVisitor visitor, bool complex, ref SpanBuilder builder)
        => visitor.VisitConstant(this, complex, ref builder);

    internal override int Accept(NodeExtensions.GetStringifiedSizeVisitor visitor, bool complex)
        => visitor.VisitConstant(this, complex);

    internal override int Accept(NodeHashVisitor visitor, ref int supply, ImmutableDictionary<UniqueString, int> context)
        => visitor.VisitConstant(this, ref supply, context);

    public override TResult Accept<TArg1, TArg2, TResult>(INodeRefVisitor<TArg1, TArg2, TResult> visitor, ref TArg1 arg1, TArg2 arg2)
        where TArg1 : struct
        => visitor.VisitConstant(this, ref arg1, arg2);

    public override TResult Accept<TArg1, TArg2, TResult>(INodeVisitor<TArg1, TArg2, TResult> visitor, TArg1 arg1, TArg2 arg2)
        => visitor.VisitConstant(this, arg1, arg2);
}