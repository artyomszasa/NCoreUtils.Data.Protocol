using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using NCoreUtils.Data.Protocol.Internal;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public sealed class CollectionContains : IFunction
{
    private static MethodInfo ContainsMethodDefinition { get; } = ReflectionHelpers
        .GetMethod<IEnumerable<int>, int, bool>(Enumerable.Contains)
        .GetGenericMethodDefinition();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Eqi(string a, string b)
        => StringComparer.InvariantCultureIgnoreCase.Equals(a, b);

    public FunctionMatch MatchFunction(IDataUtils utils, Expression expression)
    {
        if (expression is MethodCallExpression call && call.Method.IsConstructedGenericMethod
            && call.Method.GetGenericMethodDefinition() == ContainsMethodDefinition)
        {
            return new FunctionMatch(Names.Includes, call.Arguments);
        }
        return default;
    }

    public bool TryResolveFunction(
        IDataUtils util,
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        [MaybeNullWhen(false)] out IFunctionDescriptor descriptor)
    {
        if ((Eqi(Names.Contains, name) || Eqi(Names.Includes, name)) && argumentTypeConstraints.Count == 2)
        {
            var elementType = argumentTypeConstraints[1].TryGetExactType(out var exactType) ? (Type)exactType : default;
            if (elementType is not null || argumentTypeConstraints[0].TryGetElementType(util, out elementType))
            {
                descriptor = CollectionContainsDescriptor.GetOrCreate(util, elementType);
                return true;
            }
        }
        descriptor = default;
        return false;
    }
}