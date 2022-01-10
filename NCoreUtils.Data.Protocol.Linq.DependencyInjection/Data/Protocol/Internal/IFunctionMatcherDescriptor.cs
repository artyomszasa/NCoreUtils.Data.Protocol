using System;

namespace NCoreUtils.Data.Protocol.Internal;

public interface IFunctionMatcherDescriptor
{
    IFunctionMatcher GetOrCreate(IServiceProvider serviceProvider);
}