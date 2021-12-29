using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace NCoreUtils.Data.Protocol.TypeInference;

/// <summary>
/// Defines function descriptor functionality.
/// </summary>
public interface IFunctionDescriptor : IHasName
{
    /// <summary>
    /// Gets result type.
    /// </summary>
    Type ResultType { [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] get; }

    /// <summary>
    /// Gets arguments types.
    /// </summary>
    ReadOnlyConstraintedTypeList ArgumentTypes { get; }

    /// <summary>
    /// Creates expression that represents function invocation defined by the actual descriptor instance.
    /// </summary>
    /// <param name="arguments">Function arguments.</param>
    /// <returns>Expression that represents function invocation.</returns>
    Expression CreateExpression(IReadOnlyList<Expression> arguments);
}