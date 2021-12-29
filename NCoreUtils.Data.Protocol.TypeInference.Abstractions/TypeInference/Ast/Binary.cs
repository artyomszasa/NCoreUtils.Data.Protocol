using System;
using System.Collections.Generic;
using BinaryOperation = NCoreUtils.Data.Protocol.Ast.BinaryOperation;
using HashTags = NCoreUtils.Data.Protocol.Ast.HashTags;

namespace NCoreUtils.Data.Protocol.TypeInference.Ast;

public sealed class Binary<T> : Node<T>
{
    public Node<T> Left { get; }

    public BinaryOperation Operation { get; }

    public Node<T> Right { get; }

    internal Binary(T type, Node<T> left, BinaryOperation operation, Node<T> right)
        : base(type)
    {
        Left = left ?? throw new ArgumentNullException(nameof(left));
        Operation = operation;
        Right = right ?? throw new ArgumentNullException(nameof(right));
    }

    public override TResult Accept<TArg1, TArg2, TResult>(
        ITypedNodeVisitor<T, TArg1, TArg2, TResult> visitor,
        TArg1 arg1,
        TArg2 arg2)
        => visitor.VisitBinary(this, arg1, arg2);

    public override TResult Accept<TArg1, TArg2, TArg3, TOut, TResult>(
        ITypedNodeVisitor1Out<T, TArg1, TArg2, TArg3, TOut, TResult> visitor,
        TArg1 arg1,
        TArg2 arg2,
        TArg3 arg3,
        out TOut @out)
        => visitor.VisitBinary(this, arg1, arg2, arg3, out @out);

    public override Node<TTarget> Resolve<TTarget>(Func<T, TTarget> resolver)
        => Node<TTarget>.Binary(
            resolver(Type),
            Left.Resolve(resolver),
            Operation,
            Right.Resolve(resolver)
        );

    public override IEnumerable<Node<T>> GetChildren()
    {
        yield return Left;
        yield return Right;
    }

    public override int GetHashCode()
        => HashCode.Combine(HashTags.Binary, Left, Operation, Right);
}