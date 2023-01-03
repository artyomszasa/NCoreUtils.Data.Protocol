using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol.Ast;

public sealed class Call : Node
{
    public string Name { get; }

    public IReadOnlyList<Node> Arguments { get; }

    [DebuggerStepThrough]
    internal Call(string name, IReadOnlyList<Node> arguments)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
        }
        Name = name;
        Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
    }

    internal override void Accept(NodeExtensions.EmplaceVisitor visitor, bool complex, ref SpanBuilder builder)
        => visitor.VisitCall(this, complex, ref builder);

    internal override int Accept(NodeExtensions.GetStringifiedSizeVisitor visitor, bool complex)
        => visitor.VisitCall(this, complex);

    internal override int Accept(NodeHashVisitor visitor, ref int supply, ImmutableDictionary<UniqueString, int> context)
        => visitor.VisitCall(this, ref supply, context);

    public override TResult Accept<TArg1, TArg2, TResult>(INodeRefVisitor<TArg1, TArg2, TResult> visitor, ref TArg1 arg1, TArg2 arg2)
        where TArg1 : struct
        => visitor.VisitCall(this, ref arg1, arg2);

    public override TResult Accept<TArg1, TArg2, TResult>(INodeVisitor<TArg1, TArg2, TResult> visitor, TArg1 arg1, TArg2 arg2)
        => visitor.VisitCall(this, arg1, arg2);
}