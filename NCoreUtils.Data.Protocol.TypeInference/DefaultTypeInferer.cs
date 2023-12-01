using System;
using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.Ast;
using NCoreUtils.Data.Protocol.TypeInference;
using NCoreUtils.Data.Protocol.TypeInference.Ast;

namespace NCoreUtils.Data.Protocol;

public class DefaultTypeInferer(
    IDataUtils util,
    IFunctionDescriptorResolver functionResolver,
    IPropertyResolver? propertyResolver = default)
    : ITypeInferrer
{
    public IDataUtils Util { get; } = util ?? throw new ArgumentNullException(nameof(util));

    public IPropertyResolver PropertyResolver { get; } = propertyResolver ?? DefaultPropertyResolver.For(util);

    public IFunctionDescriptorResolver FunctionResolver { get; } = functionResolver ?? throw new ArgumentNullException(nameof(functionResolver));

    protected virtual Func<TypeUid, Type> CreateResolver(TypeInferenceContext context)
        => typeUid => context.InstantiateType(PropertyResolver, typeUid);

    public virtual Lambda<Type> InferTypes([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type rootType, Lambda expression)
    {
        var typedExpression = (Lambda<TypeUid>)Helpers.Idfy(expression);
        var initialContext = Helpers.CollectIds(Util, typedExpression);
        var context = initialContext.CollectConstraintsRoot(rootType, PropertyResolver, FunctionResolver, typedExpression, out var lambda);
        return (Lambda<Type>)lambda.Resolve(CreateResolver(context));
    }
}