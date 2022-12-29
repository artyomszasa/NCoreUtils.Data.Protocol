using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public abstract class CollectionOperationWithLambda : IFunction
{
    protected const int CollectionAllUid = 0;

    protected const int CollectionAnyUid = 1;

    private static ConcurrentDictionary<(int Uid, IDataUtils Util, Type Type), IFunctionDescriptor> Cache { get; } = new();

    private int Uid { get; }

    protected abstract MethodInfo GenericMethodDefinition { get; }

    protected abstract string DefaultName { get; }

    protected abstract IFunctionDescriptor CreateDescriptorFor(IDataUtils util, Type itemType);

    protected abstract bool MatchName(string name);

    internal CollectionOperationWithLambda(int uid)
        => Uid = uid;

    private IFunctionDescriptor GetOrCreateDescriptorFor(IDataUtils util, Type itemType)
    {
        if (Cache.TryGetValue((Uid, util, itemType), out var descriptor))
        {
            return descriptor;
        }
        descriptor = CreateDescriptorFor(util, itemType);
        return Cache.GetOrAdd((Uid, util, itemType), descriptor);
    }

    public FunctionMatch MatchFunction(IDataUtils util, Expression expression)
    {
        if (expression is MethodCallExpression call && call.Method.IsConstructedGenericMethod
            && call.Method.GetGenericMethodDefinition() == GenericMethodDefinition)
        {
            return new(DefaultName, call.Arguments);
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
        if (MatchName(name) && argumentTypeConstraints.Count == 2)
        {
            if (argumentTypeConstraints[0].TryGetElementType(util, out var elementType))
            {
                descriptor = GetOrCreateDescriptorFor(util, elementType);
                return true;
            }
        }
        descriptor = default;
        return false;
    }
}