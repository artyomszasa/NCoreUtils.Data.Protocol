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

public interface INodeVisitor<TArg, TResult>
{
    TResult VisitBinary(Binary binary, TArg arg);

    TResult VisitCall(Call call, TArg arg);

    TResult VisitConstant(Constant constant, TArg arg);

    TResult VisitIdentifier(Identifier identifier, TArg arg);

    TResult VisitLambda(Lambda lambda, TArg arg);

    TResult VisitMember(Member member, TArg arg);
}

public interface INodeRefVisitor<TArg, TResult>
    where TArg : struct
{
    TResult VisitBinary(Binary binary, ref TArg arg);

    TResult VisitCall(Call call, ref TArg arg);

    TResult VisitConstant(Constant constant, ref TArg arg);

    TResult VisitIdentifier(Identifier identifier, ref TArg arg);

    TResult VisitLambda(Lambda lambda, ref TArg arg);

    TResult VisitMember(Member member, ref TArg arg);
}