using System;
using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.Ast;
using NCoreUtils.Data.Protocol.TypeInference;
using NCoreUtils.Data.Protocol.TypeInference.Ast;

namespace NCoreUtils.Data.Protocol;

public interface ITypeInferrer
{
    IDataUtils Util { get; }

    IPropertyResolver PropertyResolver { get; }

    /// <summary>
    /// Infers type for all nodes in the specified expression with respect to the specified root type.
    /// </summary>
    /// <param name="rootType">Argument type of root lambda expression.</param>
    /// <param name="expression">Expression to infer types within.</param>
    /// <returns>Expression with inferred types.</returns>
    Lambda<Type> InferTypes([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type rootType, Lambda expression);
}