using System;
using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.Ast;
using NCoreUtils.Data.Protocol.TypeInference;
using NCoreUtils.Data.Protocol.TypeInference.Ast;

namespace NCoreUtils.Data.Protocol;

public class DefaultTypeInferer : ITypeInferrer
{
    public IPropertyResolver PropertyResolver { get; }

    public IFunctionDescriptorResolver FunctionResolver { get; }

    public DefaultTypeInferer(IFunctionDescriptorResolver functionResolver, IPropertyResolver? propertyResolver = default)
    {
        PropertyResolver = propertyResolver ?? DefaultPropertyResolver.Singleton;
        FunctionResolver = functionResolver ?? throw new ArgumentNullException(nameof(functionResolver));
    }

    protected virtual Func<TypeUid, Type> CreateResolver(TypeInferenceContext context)
        => typeUid => context.InstantiateType(PropertyResolver, typeUid);

    public virtual Lambda<Type> InferTypes([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type rootType, Lambda expression)
    {
        var typedExpression = (Lambda<TypeUid>)Helpers.Idfy(expression);
        var initialContext = Helpers.CollectIds(typedExpression);
        var context = initialContext.CollectConstraintsRoot(rootType, PropertyResolver, FunctionResolver, typedExpression, out var lambda);
        return (Lambda<Type>)lambda.Resolve(CreateResolver(context));
    }
}