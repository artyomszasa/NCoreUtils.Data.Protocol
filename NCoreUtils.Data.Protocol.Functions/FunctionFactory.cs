using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol;

public static class FunctionFactory
{
    private static bool TryFromProperty(
        string name,
        LambdaExpression template,
        [MaybeNullWhen(false)] out PropertyAsFunction function)
    {
        if (FunctionDescriptorFactory.TryFromProperty(name, template, out var descriptor))
        {
            function = new PropertyAsFunction(descriptor);
            return true;
        }
        function = default;
        return false;
    }

    private static bool TryFromInstanceMethod(
        string name,
        LambdaExpression template,
        [MaybeNullWhen(false)] out InstanceMethodAsFunction function)
    {
        if (FunctionDescriptorFactory.TryFromInstanceMethod(name, template, out var descriptor))
        {
            function = new InstanceMethodAsFunction(descriptor);
            return true;
        }
        function = default;
        return false;
    }

    public static PropertyAsFunction FromProperty(string name, LambdaExpression template)
        => TryFromProperty(name, template, out var function)
            ? function
            : throw new InvalidOperationException($"Unable to create property-as-function from {template}.");

    public static PropertyAsFunction FromProperty<TInstance, TProperty>(string name, Expression<Func<TInstance, TProperty>> template)
        => FromProperty(name, (LambdaExpression)template);

    public static InstanceMethodAsFunction FromInstanceMethod(string name, LambdaExpression template)
        => TryFromInstanceMethod(name, template, out var function)
            ? function
            : throw new InvalidOperationException($"Unable to create instance-method-as-function from {template}.");

    public static InstanceMethodAsFunction FromInstanceMethod<TInstance, TResult>(string name, Expression<Func<TInstance, TResult>> template)
        => FromInstanceMethod(name, (LambdaExpression)template);
}