using System;
using System.Collections.Generic;
using HashTags = NCoreUtils.Data.Protocol.Internal.NodeHashTags;

namespace NCoreUtils.Data.Protocol.TypeInference.Ast;

public sealed class Lambda<T> : Node<T>
{
    public Identifier<T> Arg { get; }

    public Node<T> Body { get; }

    internal Lambda(T type, Identifier<T> arg, Node<T> body)
        : base(type)
    {
        Arg = arg ?? throw new ArgumentNullException(nameof(arg));
        Body = body ?? throw new ArgumentNullException(nameof(body));
    }

    public override TResult Accept<TArg1, TArg2, TResult>(
        ITypedNodeVisitor<T, TArg1, TArg2, TResult> visitor,
        TArg1 arg1,
        TArg2 arg2)
        => visitor.VisitLambda(this, arg1, arg2);

    public override TResult Accept<TArg1, TArg2, TArg3, TOut, TResult>(
        ITypedNodeVisitor1Out<T, TArg1, TArg2, TArg3, TOut, TResult> visitor,
        TArg1 arg1,
        TArg2 arg2,
        TArg3 arg3,
        out TOut @out)
        => visitor.VisitLambda(this, arg1, arg2, arg3, out @out);

    public override Node<TTarget> Resolve<TTarget>(Func<T, TTarget> resolver)
        => Node<TTarget>.Lambda(
            resolver(Type),
            (Identifier<TTarget>)Arg.Resolve(resolver),
            Body.Resolve(resolver)
        );

    public override IEnumerable<Node<T>> GetChildren()
    {
        yield return Arg;
        yield return Body;
    }

    public override int GetHashCode()
        => HashCode.Combine(HashTags.Lambda, Arg, Body);
}