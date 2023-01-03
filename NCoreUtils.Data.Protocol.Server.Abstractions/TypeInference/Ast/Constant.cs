using System;
using System.Collections.Generic;
using System.Linq;
using HashTags = NCoreUtils.Data.Protocol.Internal.NodeHashTags;

namespace NCoreUtils.Data.Protocol.TypeInference.Ast;

public sealed class Constant<T> : Node<T>
{
    public string? RawValue { get; }

    internal Constant(T type, string? rawValue)
        : base(type)
        => RawValue = rawValue;

    public override TResult Accept<TArg1, TArg2, TResult>(
        ITypedNodeVisitor<T, TArg1, TArg2, TResult> visitor,
        TArg1 arg1,
        TArg2 arg2)
        => visitor.VisitConstant(this, arg1, arg2);

    public override TResult Accept<TArg1, TArg2, TArg3, TOut, TResult>(
        ITypedNodeVisitor1Out<T, TArg1, TArg2, TArg3, TOut, TResult> visitor,
        TArg1 arg1,
        TArg2 arg2,
        TArg3 arg3,
        out TOut @out)
        => visitor.VisitConstant(this, arg1, arg2, arg3, out @out);

    public override Node<TTarget> Resolve<TTarget>(Func<T, TTarget> resolver)
        => Node<TTarget>.Constant(resolver(Type), RawValue);

    public override IEnumerable<Node<T>> GetChildren()
        => Enumerable.Empty<Node<T>>();

    public override int GetHashCode()
        => HashCode.Combine(HashTags.Constant, RawValue?.GetHashCode() ?? 0);
}