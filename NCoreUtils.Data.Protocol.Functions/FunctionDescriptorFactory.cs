using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.Internal;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol;

public static class FunctionDescriptorFactory
{
    [RequiresUnreferencedCode("Caller must esure all affected types a preserved.")]
    internal static bool TryFromProperty(
        string name,
        LambdaExpression template,
        [MaybeNullWhen(false)] out PropertyAsFunctionDescriptor descriptor)
    {
        if (template.Parameters.Count == 1
            && template.Body is MemberExpression maccess
            && maccess.Member is PropertyInfo property)
        {
            descriptor = new PropertyAsFunctionDescriptor(property, property.DeclaringType ?? template.Parameters[0].Type, name);
            return true;
        }
        descriptor = default;
        return false;
    }

    [RequiresUnreferencedCode("Caller must esure all affected types a preserved.")]
    internal static bool TryFromInstanceMethod(
        string name,
        LambdaExpression template,
        [MaybeNullWhen(false)] out InstanceMethodAsFunctionDescriptor descriptor)
    {
        if (template.Parameters.Count == 1
            && template.Body is MethodCallExpression mcall
            && mcall.Object is not null)
        {
            descriptor = new InstanceMethodAsFunctionDescriptor(
                mcall.Method,
                mcall.Method.DeclaringType ?? template.Parameters[0].Type,
                name
            );
            return true;
        }
        descriptor = default;
        return false;
    }

    [RequiresUnreferencedCode("Caller must esure all affected types a preserved.")]
    public static PropertyAsFunctionDescriptor FromProperty(string name, LambdaExpression template)
        => TryFromProperty(name, template, out var descriptor)
            ? descriptor
            : throw new InvalidOperationException($"Unable to create property-as-function descriptor from {template}.");

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Type parameters have necessary attributes")]
    public static PropertyAsFunctionDescriptor FromProperty<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TInst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TProp>(string name, Expression<Func<TInst, TProp>> template)
        => FromProperty(name, (LambdaExpression)template);

    [RequiresUnreferencedCode("Caller must esure all affected types a preserved.")]
    public static InstanceMethodAsFunctionDescriptor FromInstanceMethod(string name, LambdaExpression template)
        => TryFromInstanceMethod(name, template, out var descriptor)
            ? descriptor
            : throw new InvalidOperationException($"Unable to create instance-method-as-function descriptor from {template}.");

    [RequiresUnreferencedCode("Caller must esure all affected types a preserved.")]
    public static InstanceMethodAsFunctionDescriptor FromInstanceMethod<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TInstance, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResult>(string name, Expression<Func<TInstance, TResult>> template)
        => FromInstanceMethod(name, (LambdaExpression)template);

}