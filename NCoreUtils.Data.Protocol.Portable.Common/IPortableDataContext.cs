using System;
using System.Collections.Generic;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol;

public interface IPortableDataContext
{
    IEnumerable<ITypeDescriptor> GetTypeDescriptors();

    IEnumerable<(Type ArgType, Type ResType, Type LambdaType)> GetLambdaTypes();
}