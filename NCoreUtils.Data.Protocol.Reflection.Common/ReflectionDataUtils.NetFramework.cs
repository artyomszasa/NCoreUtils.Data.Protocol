using System;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol;

public partial class ReflectionDataUtils
{
    public bool IsReference(Type type) => DefaultDataUtilsImplementation.IsReference(this, type);

    public bool IsNullable(Type type) => DefaultDataUtilsImplementation.IsNullable(this, type);

    public bool IsMaybe(Type type) => DefaultDataUtilsImplementation.IsMaybe(this, type);

    public bool IsOptional(Type type) => DefaultDataUtilsImplementation.IsOptional(this, type);

    public bool IsReferenceOrNullable(Type type) => DefaultDataUtilsImplementation.IsReferenceOrNullable(this, type);

    public bool IsArithmeticOrEnum(Type type) => DefaultDataUtilsImplementation.IsArithmeticOrEnum(this, type);

    public bool IsLambda(Type type) => DefaultDataUtilsImplementation.IsLambda(this, type);

    public bool IsArray(Type type) => DefaultDataUtilsImplementation.IsArray(this, type);

    public bool IsEnumerable(Type type) => DefaultDataUtilsImplementation.IsEnumerable(this, type);
}