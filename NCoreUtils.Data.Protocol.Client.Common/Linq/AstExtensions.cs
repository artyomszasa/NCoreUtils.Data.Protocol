using NCoreUtils.Data.Protocol.Ast;

namespace NCoreUtils.Data.Protocol.Linq;

internal static class AstExtensions
{
    public static Lambda AndAlso(this Lambda a, Lambda b)
        => Node.Lambda(
            arg: a.Arg,
            body: Node.Binary(
                left: a.Body,
                operation: BinaryOperation.AndAlso,
                right: b.Body.SubstituteParameter(b.Arg.Value, a.Arg)
            )
        );
}