using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using NCoreUtils.Data.Protocol.Internal;
using NCoreUtils.Memory;

namespace NCoreUtils.Data.Protocol.Ast;

public abstract class Node : IEquatable<Node>, ISpanExactEmplaceable
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

    [DebuggerStepThrough]
    public static Lambda Lambda(Identifier arg, Node body)
        => new(arg, body);

    [DebuggerStepThrough]
    public static Binary Binary(Node left, BinaryOperation operation, Node right)
        => new(left, operation, right);

    [DebuggerStepThrough]
    public static Call Call(string name, IReadOnlyList<Node> arguments)
        => new(name, arguments);

    [DebuggerStepThrough]
    public static Call Call(string name, params Node[] arguments)
        => Call(name, (IReadOnlyList<Node>)arguments);

    [DebuggerStepThrough]
    public static Member Member(Node instance, string memberName)
        => new(instance, memberName);

    [DebuggerStepThrough]
    public static Constant Constant(string? rawValue)
        => new(rawValue);

    [DebuggerStepThrough]
    public static Identifier Identifier(UniqueString value)
        => new(value);

    [DebuggerStepThrough]
    internal Node() { }

    internal abstract void Accept(NodeExtensions.EmplaceVisitor visitor, bool complex, ref SpanBuilder builder);

    internal abstract int Accept(NodeExtensions.GetStringifiedSizeVisitor visitor, bool complex);

    internal abstract int Accept(NodeHashVisitor visitor, ref int supply, ImmutableDictionary<UniqueString, int> context);

    public abstract TResult Accept<TArg1, TArg2, TResult>(INodeRefVisitor<TArg1, TArg2, TResult> visitor, ref TArg1 arg1, TArg2 arg2)
        where TArg1 : struct;

    public abstract TResult Accept<TArg1, TArg2, TResult>(INodeVisitor<TArg1, TArg2, TResult> visitor, TArg1 arg1, TArg2 arg2);

    public bool Equals(Node? node)
        => node is not null && DeepEq(this, node, ImmutableDictionary<UniqueString, UniqueString>.Empty);

    public override bool Equals(object? obj)
        => obj is Node other && Equals(other);

    public override int GetHashCode()
    {
        var supply = 0;
        return Accept(NodeHashVisitor.Singleton, ref supply, ImmutableDictionary<UniqueString, int>.Empty);
    }

    public override string ToString()
        => this.ToStringUsingArrayPool();

    #region emplaceable

    public int GetEmplaceBufferSize()
        => this.CalculateStringifiedSize();

#if !NET6_0_OR_GREATER
    bool ISpanEmplaceable.TryGetEmplaceBufferSize(out int minimumBufferSize)
    {
        minimumBufferSize = this.CalculateStringifiedSize();
        return true;
    }

    bool ISpanEmplaceable.TryFormat(System.Span<char> destination, out int charsWritten, System.ReadOnlySpan<char> format, System.IFormatProvider? provider)
        => TryEmplace(destination, out charsWritten);
#endif

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

    public string ToString(string? format, IFormatProvider? formatProvider)
        => ToString();

    #endregion
}