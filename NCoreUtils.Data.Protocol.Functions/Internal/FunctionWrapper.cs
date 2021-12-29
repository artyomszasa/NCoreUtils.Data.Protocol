using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.Internal
{
    public class FunctionWrapper : IFunction
    {
        public IFunction Function { get; }

        public FunctionWrapper(IFunction function)
            => Function = function ?? throw new ArgumentNullException(nameof(function));

        public bool TryResolveFunction(
            string name,
            TypeVariable resultTypeConstraints,
            IReadOnlyList<TypeVariable> argumentTypeConstraints,
            [MaybeNullWhen(false)] out IFunctionDescriptor descriptor)
            => Function.TryResolveFunction(name, resultTypeConstraints, argumentTypeConstraints, out descriptor);

        public FunctionMatch MatchFunction(Expression expression)
            => Function.MatchFunction(expression);
    }
}