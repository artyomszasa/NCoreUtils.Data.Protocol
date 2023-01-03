namespace NCoreUtils.Data.Protocol.TypeInference.Ast;

public interface ITypedNodeVisitor<T, TArg1, TArg2, TResult>
{
    TResult VisitBinary(Binary<T> binary, TArg1 arg1, TArg2 arg2);

    TResult VisitCall(Call<T> call, TArg1 arg1, TArg2 arg2);

    TResult VisitConstant(Constant<T> constant, TArg1 arg1, TArg2 arg2);

    TResult VisitIdentifier(Identifier<T> identifier, TArg1 arg1, TArg2 arg2);

    TResult VisitLambda(Lambda<T> lambda, TArg1 arg1, TArg2 arg2);

    TResult VisitMember(Member<T> member, TArg1 arg1, TArg2 arg2);
}

public interface ITypedNodeVisitor1Out<T, TArg1, TArg2, TArg3, TOut, TResult>
{
    TResult VisitBinary(Binary<T> binary, TArg1 arg1, TArg2 arg2, TArg3 arg3, out TOut @out);

    TResult VisitCall(Call<T> call, TArg1 arg1, TArg2 arg2, TArg3 arg3, out TOut @out);

    TResult VisitConstant(Constant<T> constant, TArg1 arg1, TArg2 arg2, TArg3 arg3, out TOut @out);

    TResult VisitIdentifier(Identifier<T> identifier, TArg1 arg1, TArg2 arg2, TArg3 arg3, out TOut @out);

    TResult VisitLambda(Lambda<T> lambda, TArg1 arg1, TArg2 arg2, TArg3 arg3, out TOut @out);

    TResult VisitMember(Member<T> member, TArg1 arg1, TArg2 arg2, TArg3 arg3, out TOut @out);
}