using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.Internal;

public class FunctionWrapper : IFunction
{
    public IFunction Function { get; }

    public FunctionWrapper(IFunction function)
        => Function = function ?? throw new ArgumentNullException(nameof(function));

    public bool TryResolveFunction(
        IDataUtils util,
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        [MaybeNullWhen(false)] out IFunctionDescriptor descriptor)
        => Function.TryResolveFunction(util, name, resultTypeConstraints, argumentTypeConstraints, out descriptor);

    public FunctionMatch MatchFunction(IDataUtils utils, Expression expression)
        => Function.MatchFunction(utils, expression);
}