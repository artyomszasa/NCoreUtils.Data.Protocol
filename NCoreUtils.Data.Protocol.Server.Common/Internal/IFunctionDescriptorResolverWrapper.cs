using System;

namespace NCoreUtils.Data.Protocol.Internal;

/// <summary>
/// Wrapper interface used to register resolvers used by the composite function descriptor resolver.
/// </summary>
internal interface IFunctionDescriptorResolverWrapper : IFunctionDescriptorResolver, IDisposable { }