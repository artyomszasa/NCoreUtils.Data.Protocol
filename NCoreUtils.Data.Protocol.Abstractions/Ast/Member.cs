using System;

namespace NCoreUtils.Data.Protocol.Ast;

public sealed class Member : Node
{
    public Node Instance { get; }

    public string MemberName { get; }

    internal Member(Node instance, string memberName)
    {
        if (string.IsNullOrWhiteSpace(memberName))
        {
            throw new ArgumentException($"'{nameof(memberName)}' cannot be null or whitespace.", nameof(memberName));
        }
        Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        MemberName = memberName;
    }

    internal override void Accept(NodeExtensions.EmplaceVisitor visitor, bool complex, ref SpanBuilder builder)
        => visitor.VisitMember(this, complex, ref builder);

    internal override int Accept(NodeExtensions.GetStringifiedSizeVisitor visitor, bool complex)
        => visitor.VisitMember(this, complex);

    public override TResult Accept<TArg, TResult>(INodeVisitor<TArg, TResult> visitor, TArg arg)
        => visitor.VisitMember(this, arg);

    public override TResult Accept<TArg, TResult>(INodeRefVisitor<TArg, TResult> visitor, ref TArg arg)
        where TArg : struct
        => visitor.VisitMember(this, ref arg);

    public override TResult Accept<TArg1, TArg2, TResult>(INodeRefVisitor<TArg1, TArg2, TResult> visitor, ref TArg1 arg1, TArg2 arg2)
        where TArg1 : struct
        => visitor.VisitMember(this, ref arg1, arg2);

    public override TResult Accept<TArg1, TArg2, TResult>(INodeVisitor<TArg1, TArg2, TResult> visitor, TArg1 arg1, TArg2 arg2)
        => visitor.VisitMember(this, arg1, arg2);

    public override int GetHashCode()
        => HashCode.Combine(HashTags.Member, Instance, MemberName);
}