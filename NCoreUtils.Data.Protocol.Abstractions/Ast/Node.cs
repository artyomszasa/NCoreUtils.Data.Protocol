using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using NCoreUtils.Memory;

namespace NCoreUtils.Data.Protocol.Ast;

#pragma warning disable CS0659
// NOTE: GetHasCode overriden in every derived class
public abstract class Node : IEquatable<Node>, IEmplaceable<Node>
{
    private static bool SequenceDeepEq(IReadOnlyList<Node> @as, IReadOnlyList<Node> bs, ImmutableDictionary<UniqueString, UniqueString> context)
    {
        if (@as.Count != bs.Count)
        {
            return false;
        }
        for (var i = 0; i < @as.Count; ++i)
        {
            if (!DeepEq(@as[i], bs[i], context))
            {
                return false;
            }
        }
        return true;
    }

    private static bool DeepEq(Node a, Node b, ImmutableDictionary<UniqueString, UniqueString> context)
        => (a, b) switch
        {
            (Lambda la, Lambda lb) => DeepEq(la.Body, lb.Body, context.Add(la.Arg.Value, lb.Arg.Value)),
            (Binary ba, Binary bb) when ba.Operation == bb.Operation =>
                DeepEq(ba.Left, bb.Left, context) && DeepEq(ba.Right, bb.Right, context),
            (Call ca, Call cb) when StringComparer.InvariantCultureIgnoreCase.Equals(ca.Name, cb.Name) =>
                SequenceDeepEq(ca.Arguments, cb.Arguments, context),
            (Member ma, Member mb) when StringComparer.InvariantCultureIgnoreCase.Equals(ma.MemberName, mb.MemberName) =>
                DeepEq(ma.Instance, mb.Instance, context),
            (Constant ca, Constant cb) => ca.RawValue == cb.RawValue,
            (Identifier ia, Identifier ib) => context.TryGetValue(ia.Value, out var mapped) && mapped == ib.Value,
            _ => false
        };

    public static Lambda Lambda(Identifier arg, Node body)
        => new(arg, body);

    public static Binary Binary(Node left, BinaryOperation operation, Node right)
        => new(left, operation, right);

    public static Call Call(string name, IReadOnlyList<Node> arguments)
        => new(name, arguments);

    public static Call Call(string name, params Node[] arguments)
        => Call(name, (IReadOnlyList<Node>)arguments);

    public static Member Member(Node instance, string memberName)
        => new(instance, memberName);

    public static Constant Constant(string? rawValue)
        => new(rawValue);

    public static Identifier Identifier(UniqueString value)
        => new(value);

    internal Node() { }

    internal abstract void Accept(NodeExtensions.EmplaceVisitor visitor, bool complex, ref SpanBuilder builder);

    internal abstract int Accept(NodeExtensions.GetStringifiedSizeVisitor visitor, bool complex);

    public abstract TResult Accept<TArg, TResult>(INodeVisitor<TArg, TResult> visitor, TArg arg);

    public abstract TResult Accept<TArg, TResult>(INodeRefVisitor<TArg, TResult> visitor, ref TArg arg)
        where TArg : struct;

    public abstract TResult Accept<TArg1, TArg2, TResult>(INodeVisitor<TArg1, TArg2, TResult> visitor, TArg1 arg1, TArg2 arg2);

    public bool Equals(Node? node)
        => node is not null && DeepEq(this, node, ImmutableDictionary<UniqueString, UniqueString>.Empty);

    public override bool Equals(object? obj)
        => obj is Node other && Equals(other);

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