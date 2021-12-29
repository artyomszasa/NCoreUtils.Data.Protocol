using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace NCoreUtils.Data.Protocol.TypeInference;

public interface IProperty
{
    Type PropertyType
    {
        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        get;
    }

    Expression CreateExpression(Expression instance);
}