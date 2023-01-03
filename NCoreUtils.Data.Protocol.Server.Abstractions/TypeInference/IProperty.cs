using System;
using System.Linq.Expressions;

namespace NCoreUtils.Data.Protocol.TypeInference;

public interface IProperty
{
    Type PropertyType { get; }

    Expression CreateExpression(Expression instance);
}