using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using NCoreUtils.Memory;
using BinaryOperation = NCoreUtils.Data.Protocol.Ast.BinaryOperation;
using UniqueString = NCoreUtils.Data.Protocol.Ast.UniqueString;

namespace NCoreUtils.Data.Protocol.TypeInference.Ast;

#pragma warning disable CS0659
// NOTE: GetHasCode overriden in every derived class
public abstract class Node<T> // : IEquatable<Node<T>>, IEmplaceable<Node<T>>
{
    // private static bool SequenceDeepEq(
    //     IReadOnlyList<Node<T>> @as,
    //     IReadOnlyList<Node<T>> bs,
    //     ImmutableDictionary<UniqueString, UniqueString> context)
    // {
    //     if (@as.Count != bs.Count)
    //     {
    //         return false;
    //     }
    //     for (var i = 0; i < @as.Count; ++i)
    //     {
    //         if (!DeepEq(@as[i], bs[i], context))
    //         {
    //             return false;
    //         }
    //     }
    //     return true;
    // }

    // private static bool DeepEq(Node<T> a, Node<T> b, ImmutableDictionary<UniqueString, UniqueString> context)
    //     => (a, b) switch
    //     {
    //         (Lambda<T> la, Lambda<T> lb) => DeepEq(la.Body, lb.Body, context.Add(la.Arg.Value, lb.Arg.Value)),
    //         (Binary ba, Binary bb) when ba.Operation == bb.Operation =>
    //             DeepEq(ba.Left, bb.Left, context) && DeepEq(ba.Right, bb.Right, context),
    //         (Call ca, Call cb) when StringComparer.InvariantCultureIgnoreCase.Equals(ca.Name, cb.Name) =>
    //             SequenceDeepEq(ca.Arguments, cb.Arguments, context),
    //         (Member ma, Member mb) when StringComparer.InvariantCultureIgnoreCase.Equals(ma.MemberName, mb.MemberName) =>
    //             DeepEq(ma.Instance, mb.Instance, context),
    //         (Constant ca, Constant cb) => ca.RawValue == cb.RawValue,
    //         (Identifier ia, Identifier ib) => context.TryGetValue(ia.Value, out var mapped) && mapped == ib.Value,
    //         _ => false
    //     };

    public static Lambda<T> Lambda(T type, Identifier<T> arg, Node<T> body)
        => new(type, arg, body);

    public static Binary<T> Binary(T type, Node<T> left, BinaryOperation operation, Node<T> right)
        => new(type, left, operation, right);

    public static Call<T> Call(T type, IFunctionDescriptor descriptor, IReadOnlyList<Node<T>> arguments)
        => new(type, descriptor, arguments);

    public static Call<T> Call(T type, IFunctionDescriptor descriptor, params Node<T>[] arguments)
        => Call(type, descriptor, (IReadOnlyList<Node<T>>)arguments);

    public static Member<T> Member(T type, Node<T> instance, string memberName)
        => new(type, instance, memberName);

    public static Constant<T> Constant(T type, string? rawValue)
        => new(type, rawValue);

    public static Identifier<T> Identifier(T type, UniqueString value)
        => new(type, value);

    public T Type { get; }

    internal Node(T type)
        => Type = type;

    public abstract TResult Accept<TArg1, TArg2, TResult>(
        ITypedNodeVisitor<T, TArg1, TArg2, TResult> visitor,
        TArg1 arg1,
        TArg2 arg2
    );

    public abstract TResult Accept<TArg1, TArg2, TArg3, TOut, TResult>(
        ITypedNodeVisitor1Out<T, TArg1, TArg2, TArg3, TOut, TResult> visitor,
        TArg1 arg1,
        TArg2 arg2,
        TArg3 arg3,
        out TOut @out
    );

    public abstract Node<TTarget> Resolve<TTarget>(Func<T, TTarget> resolver);

    // public bool Equals(Node<T>? node)
    //     => node is not null && DeepEq(this, node, ImmutableDictionary<UniqueString, UniqueString>.Empty);

    // public override bool Equals(object? obj)
    //     => obj is Node<T> other && Equals(other);

    public abstract IEnumerable<Node<T>> GetChildren();

    public override string ToString()
    {
        var requiredSize = this.CalculateStringifiedSize();
        var buffer = ArrayPool<char>.Shared.Rent(requiredSize);
        try
        {
            var size = this.EmplaceTo(buffer);
            return buffer.AsSpan()[..size].ToString();
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }

    #region emplaceable

    public int Emplace(Span<char> span)
    {
        var requiredSize = this.CalculateStringifiedSize();
        if (span.Length >= requiredSize)
        {
            return this.EmplaceTo(span);
        }
        throw new InsufficientBufferSizeException(span, requiredSize);
    }

    public bool TryEmplace(Span<char> span, out int used)
    {
        var requiredSize = this.CalculateStringifiedSize();
        if (span.Length >= requiredSize)
        {
            used = this.EmplaceTo(span);
            return true;
        }
        used = default;
        return false;
    }

    #endregion
}
#pragma warning restore CS0659