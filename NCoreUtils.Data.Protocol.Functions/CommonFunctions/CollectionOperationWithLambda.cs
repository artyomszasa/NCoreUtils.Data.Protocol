using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public abstract class CollectionOperationWithLambda<TDescriptor> : IFunction
    where TDescriptor : IFunctionDescriptor
{
    private static ConcurrentDictionary<Type, TDescriptor> Cache { get; } = new();

    protected abstract MethodInfo GenericMethodDefinition { get; }

    protected abstract string DefaultName { get; }

    protected abstract TDescriptor CreateDescriptorFor(Type itemType);

    protected abstract bool MatchName(string name);

    internal CollectionOperationWithLambda() { }

    private TDescriptor GetOrCreateDescriptorFor(Type itemType)
    {
        // avoid allocating Func<,> if the descriptor has already been populated.
        if (Cache.TryGetValue(itemType, out var descriptor))
        {
            return descriptor;
        }
        return Cache.GetOrAdd(itemType, CreateDescriptorFor);

    }

    public FunctionMatch MatchFunction(Expression expression)
    {
        if (expression is MethodCallExpression call && call.Method.IsConstructedGenericMethod
            && call.Method.GetGenericMethodDefinition() == GenericMethodDefinition)
        {
            return new(DefaultName, call.Arguments);
        }
        return default;
    }

    public bool TryResolveFunction(
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        [MaybeNullWhen(false)] out IFunctionDescriptor descriptor)
    {
        if (MatchName(name) && argumentTypeConstraints.Count == 2)
        {
            var maybeElementType = argumentTypeConstraints[0].Match(
                type => Helpers.TryGetElementType(type, out var elementType)
                    ? elementType.Just()
                    : default,
                constraints => Helpers.TryGetElementType(constraints, out var elementType)
                    ? elementType.Just()
                    : default
            );
            if (maybeElementType.TryGetValue(out var elementType) && elementType is not null)
            {
                descriptor = GetOrCreateDescriptorFor(elementType);
                return true;
            }
        }
        descriptor = default;
        return false;
    }
}