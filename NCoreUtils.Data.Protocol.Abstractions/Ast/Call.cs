using System;
using System.Collections.Generic;

namespace NCoreUtils.Data.Protocol.Ast;

public sealed class Call : Node
{
    public string Name { get; }

    public IReadOnlyList<Node> Arguments { get; }

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

    public override TResult Accept<TArg, TResult>(INodeVisitor<TArg, TResult> visitor, TArg arg)
        => visitor.VisitCall(this, arg);

    public override TResult Accept<TArg, TResult>(INodeRefVisitor<TArg, TResult> visitor, ref TArg arg)
        where TArg : struct
        => visitor.VisitCall(this, ref arg);

    public override TResult Accept<TArg1, TArg2, TResult>(INodeVisitor<TArg1, TArg2, TResult> visitor, TArg1 arg1, TArg2 arg2)
        => visitor.VisitCall(this, arg1, arg2);

    public override int GetHashCode()
    {
        var builder = new HashCode();
        builder.Add(HashTags.Call);
        builder.Add(StringComparer.InvariantCultureIgnoreCase.GetHashCode(Name));
        builder.Add(Arguments.Count);
        foreach (var arg in Arguments)
        {
            builder.Add(arg);
        }
        return builder.ToHashCode();
    }
}