using System;
using System.Collections.Generic;
using HashTags = NCoreUtils.Data.Protocol.Internal.NodeHashTags;

namespace NCoreUtils.Data.Protocol.TypeInference.Ast;

public sealed class Call<T> : Node<T>
{
    public IFunctionDescriptor Descriptor { get; }

    public IReadOnlyList<Node<T>> Arguments { get; }

    internal Call(T type, IFunctionDescriptor descriptor, IReadOnlyList<Node<T>> arguments)
        : base(type)
    {
        Descriptor = descriptor;
        Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
    }
    public override Node<TTarget> Resolve<TTarget>(Func<T, TTarget> resolver)
        => Node<TTarget>.Call(
            resolver(Type),
            Descriptor,
            Arguments.MapToArray(node => node.Resolve(resolver))
        );

    public override TResult Accept<TArg1, TArg2, TResult>(
        ITypedNodeVisitor<T, TArg1, TArg2, TResult> visitor,
        TArg1 arg1,
        TArg2 arg2)
        => visitor.VisitCall(this, arg1, arg2);

    public override TResult Accept<TArg1, TArg2, TArg3, TOut, TResult>(
        ITypedNodeVisitor1Out<T, TArg1, TArg2, TArg3, TOut, TResult> visitor,
        TArg1 arg1,
        TArg2 arg2,
        TArg3 arg3,
        out TOut @out)
        => visitor.VisitCall(this, arg1, arg2, arg3, out @out);

    public override IEnumerable<Node<T>> GetChildren()
        => Arguments;

    public override int GetHashCode()
    {
        var builder = new HashCode();
        builder.Add(HashTags.Call);
        builder.Add(Descriptor);
        builder.Add(Arguments.Count);
        foreach (var arg in Arguments)
        {
            builder.Add(arg);
        }
        return builder.ToHashCode();
    }
}