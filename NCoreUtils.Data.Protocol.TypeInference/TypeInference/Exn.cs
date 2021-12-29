using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NCoreUtils.Data.Protocol.TypeInference;

internal static class Exn
{
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UnableToResolveCall(
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentsTypeConstraints)
        => throw new ProtocolTypeInferenceException(
            $"Unable to resolve function call with (Name = {name}, Result = {resultTypeConstraints}, Args = [{string.Join(", ", argumentsTypeConstraints)}])."
        );

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Style", "IDE0060", MessageId = "dummy")]
    public static TypeInferenceContext LambdaArgumentExpected<T>(
        int index,
        IFunctionDescriptor descriptor,
        out Ast.Node<T> dummy)
        => throw new ProtocolTypeInferenceException(
            $"Invalid argument (non-lambda) at {index} in function call with (Name = {descriptor.Name}, Result = {descriptor.ResultType}, Args = [{string.Join(", ", descriptor.ArgumentTypes)}])"
        );
}