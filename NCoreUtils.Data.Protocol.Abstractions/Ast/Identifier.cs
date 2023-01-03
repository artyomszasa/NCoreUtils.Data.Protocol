using System;
using System.Collections.Immutable;
using System.Diagnostics;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol.Ast;

public sealed class Identifier : Node
{
    public UniqueString Value { get; }

    [DebuggerStepThrough]
    internal Identifier(UniqueString value)
        => Value = value ?? throw new ArgumentNullException(nameof(value));

    internal override void Accept(NodeExtensions.EmplaceVisitor visitor, bool complex, ref SpanBuilder builder)
        => visitor.VisitIdentifier(this, complex, ref builder);

    internal override int Accept(NodeExtensions.GetStringifiedSizeVisitor visitor, bool complex)
        => visitor.VisitIdentifier(this, complex);

    internal override int Accept(NodeHashVisitor visitor, ref int supply, ImmutableDictionary<UniqueString, int> context)
        => visitor.VisitIdentifier(this, ref supply, context);

    public override TResult Accept<TArg1, TArg2, TResult>(INodeRefVisitor<TArg1, TArg2, TResult> visitor, ref TArg1 arg1, TArg2 arg2)
        where TArg1 : struct
        => visitor.VisitIdentifier(this, ref arg1, arg2);

    public override TResult Accept<TArg1, TArg2, TResult>(INodeVisitor<TArg1, TArg2, TResult> visitor, TArg1 arg1, TArg2 arg2)
        => visitor.VisitIdentifier(this, arg1, arg2);
}