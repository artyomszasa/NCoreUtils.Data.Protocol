using System;
using System.Collections.Generic;
using HashTags = NCoreUtils.Data.Protocol.Ast.HashTags;

namespace NCoreUtils.Data.Protocol.TypeInference.Ast;

public sealed class Member<T> : Node<T>
{
    public Node<T> Instance { get; }

    public string MemberName { get; }

    internal Member(T type, Node<T> instance, string memberName)
        : base(type)
    {
        if (string.IsNullOrWhiteSpace(memberName))
        {
            throw new ArgumentException($"'{nameof(memberName)}' cannot be null or whitespace.", nameof(memberName));
        }
        Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        MemberName = memberName;
    }

    public override TResult Accept<TArg1, TArg2, TResult>(
        ITypedNodeVisitor<T, TArg1, TArg2, TResult> visitor,
        TArg1 arg1,
        TArg2 arg2)
        => visitor.VisitMember(this, arg1, arg2);

    public override TResult Accept<TArg1, TArg2, TArg3, TOut, TResult>(
        ITypedNodeVisitor1Out<T, TArg1, TArg2, TArg3, TOut, TResult> visitor,
        TArg1 arg1,
        TArg2 arg2,
        TArg3 arg3,
        out TOut @out)
        => visitor.VisitMember(this, arg1, arg2, arg3, out @out);

    public override Node<TTarget> Resolve<TTarget>(Func<T, TTarget> resolver)
        => Node<TTarget>.Member(
            resolver(Type),
            Instance.Resolve(resolver),
            MemberName
        );

    public override IEnumerable<Node<T>> GetChildren()
    {
        yield return Instance;
    }

    public override int GetHashCode()
        => HashCode.Combine(HashTags.Member, Instance, MemberName);
}