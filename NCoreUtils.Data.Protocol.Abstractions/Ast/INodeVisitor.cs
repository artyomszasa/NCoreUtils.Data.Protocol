namespace NCoreUtils.Data.Protocol.Ast;

public interface INodeVisitor<TArg1, TArg2, TResult>
{
    TResult VisitBinary(Binary binary, TArg1 arg1, TArg2 arg2);

    TResult VisitCall(Call call, TArg1 arg1, TArg2 arg2);

    TResult VisitConstant(Constant constant, TArg1 arg1, TArg2 arg2);

    TResult VisitIdentifier(Identifier identifier, TArg1 arg1, TArg2 arg2);

    TResult VisitLambda(Lambda lambda, TArg1 arg1, TArg2 arg2);

    TResult VisitMember(Member member, TArg1 arg1, TArg2 arg2);
}

public interface INodeRefVisitor<TArg1, TArg2, TResult>
    where TArg1 : struct
{
    TResult VisitBinary(Binary binary, ref TArg1 arg1, TArg2 arg2);

    TResult VisitCall(Call call, ref TArg1 arg1, TArg2 arg2);

    TResult VisitConstant(Constant constant, ref TArg1 arg1, TArg2 arg2);

    TResult VisitIdentifier(Identifier identifier, ref TArg1 arg1, TArg2 arg2);

    TResult VisitLambda(Lambda lambda, ref TArg1 arg1, TArg2 arg2);

    TResult VisitMember(Member member, ref TArg1 arg1, TArg2 arg2);
}