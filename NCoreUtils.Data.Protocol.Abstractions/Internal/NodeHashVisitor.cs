using System;
using System.Collections.Immutable;
using NCoreUtils.Data.Protocol.Ast;

namespace NCoreUtils.Data.Protocol.Internal;

internal sealed class NodeHashVisitor
{
    public static NodeHashVisitor Singleton { get; } = new NodeHashVisitor();

    private NodeHashVisitor() { }

    public int VisitBinary(Binary binary, ref int supply, ImmutableDictionary<UniqueString, int> context)
        => HashCode.Combine(
            NodeHashTags.Binary,
            binary.Left.Accept(this, ref supply, context),
            binary.Operation,
            binary.Right.Accept(this, ref supply, context)
        );

    public int VisitCall(Call call, ref int supply, ImmutableDictionary<UniqueString, int> context)
    {
        var builder = new HashCode();
        builder.Add(NodeHashTags.Call);
        builder.Add(StringComparer.InvariantCultureIgnoreCase.GetHashCode(call.Name));
        builder.Add(call.Arguments.Count);
        foreach (var arg in call.Arguments)
        {
            builder.Add(arg.Accept(this, ref supply, context));
        }
        return builder.ToHashCode();
    }

    public int VisitConstant(Constant constant, ref int supply, ImmutableDictionary<UniqueString, int> context)
        => HashCode.Combine(NodeHashTags.Constant, constant.RawValue?.GetHashCode() ?? 0);

    public int VisitIdentifier(Identifier identifier, ref int supply, ImmutableDictionary<UniqueString, int> context)
        => HashCode.Combine(
            NodeHashTags.Identifier,
            context.TryGetValue(identifier.Value, out var id) ? id : identifier.Value.GetHashCode()
        );

    public int VisitLambda(Lambda lambda, ref int supply, ImmutableDictionary<UniqueString, int> context)
    {
        var id = supply++;
        return HashCode.Combine(
            NodeHashTags.Lambda,
            id,
            lambda.Body.Accept(this, ref supply, context.Add(lambda.Arg.Value, id))
        );
    }

    public int VisitMember(Member member, ref int supply, ImmutableDictionary<UniqueString, int> context)
        => HashCode.Combine(
            NodeHashTags.Member,
            member.Instance.Accept(this, ref supply, context),
            StringComparer.InvariantCultureIgnoreCase.GetHashCode(member.MemberName)
        );
}