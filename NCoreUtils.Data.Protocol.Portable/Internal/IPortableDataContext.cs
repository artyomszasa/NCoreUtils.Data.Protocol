using System;
using System.Collections.Generic;

namespace NCoreUtils.Data.Protocol.Internal;

public interface IPortableDataContext
{
    IEnumerable<ITypeDescriptor> GetTypeDescriptors();

    IEnumerable<(Type ArgType, Type ResType, Type LambdaType)> GetLambdaTypes();
}