using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol;

public static class FunctionFactory
{
    [RequiresUnreferencedCode("Caller must esure all affected types a preserved.")]
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

    [RequiresUnreferencedCode("Caller must esure all affected types a preserved.")]
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

    [RequiresUnreferencedCode("Caller must esure all affected types a preserved.")]
    public static PropertyAsFunction FromProperty(string name, LambdaExpression template)
        => TryFromProperty(name, template, out var function)
            ? function
            : throw new InvalidOperationException($"Unable to create property-as-function from {template}.");

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Type parameters have necessary attributes")]
    public static PropertyAsFunction FromProperty<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TInstance, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TProperty>(string name, Expression<Func<TInstance, TProperty>> template)
        => FromProperty(name, (LambdaExpression)template);

    [RequiresUnreferencedCode("Caller must esure all affected types a preserved.")]
    public static InstanceMethodAsFunction FromInstanceMethod(string name, LambdaExpression template)
        => TryFromInstanceMethod(name, template, out var function)
            ? function
            : throw new InvalidOperationException($"Unable to create instance-method-as-function from {template}.");

    [RequiresUnreferencedCode("Caller must esure all affected types a preserved.")]
    public static InstanceMethodAsFunction FromInstanceMethod<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TInstance, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]TResult>(string name, Expression<Func<TInstance, TResult>> template)
        => FromInstanceMethod(name, (LambdaExpression)template);
}