using System;
using System.Collections.Generic;
using System.Linq;
using HashTags = NCoreUtils.Data.Protocol.Ast.HashTags;
using UniqueString = NCoreUtils.Data.Protocol.Ast.UniqueString;

namespace NCoreUtils.Data.Protocol.TypeInference.Ast;

public sealed class Identifier<T> : Node<T>
{
    public UniqueString Value { get; }

    internal Identifier(T type, UniqueString value)
        : base(type)
        => Value = value ?? throw new ArgumentNullException(nameof(value));

    public override TResult Accept<TArg1, TArg2, TResult>(
        ITypedNodeVisitor<T, TArg1, TArg2, TResult> visitor,
        TArg1 arg1,
        TArg2 arg2)
        => visitor.VisitIdentifier(this, arg1, arg2);

    public override TResult Accept<TArg1, TArg2, TArg3, TOut, TResult>(
        ITypedNodeVisitor1Out<T, TArg1, TArg2, TArg3, TOut, TResult> visitor,
        TArg1 arg1,
        TArg2 arg2,
        TArg3 arg3,
        out TOut @out)
        => visitor.VisitIdentifier(this, arg1, arg2, arg3, out @out);

    public override Node<TTarget> Resolve<TTarget>(Func<T, TTarget> resolver)
        => Node<TTarget>.Identifier(resolver(Type), Value);

    public override IEnumerable<Node<T>> GetChildren()
        => Enumerable.Empty<Node<T>>();

    public override int GetHashCode()
        => HashCode.Combine(HashTags.Identifier, Value.GetHashCode());
}