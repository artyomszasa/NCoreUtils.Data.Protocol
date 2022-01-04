using System.Collections.Generic;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol;

public interface IAmbigousFunctionDescriptorResolver : IFunctionDescriptorResolver
{
    /// <summary>
    /// Attempt to resolve all potentially compatible functions based on the specified parameters.
    /// </summary>
    /// <param name="name">Function name.</param>
    /// <param name="resultTypeConstraints">Deducted type or type constraints of the return type.</param>
    /// <param name="argumentTypeConstraints">Deducted types or type constraints of the argument types.</param>
    /// <param name="descriptors">Collection to add potentially compatible functions.</param>
    /// <returns>
    /// <c>true</c> if at least one potentially compatible function has been resolved, <c>false</c> otherwise.
    /// </returns>
    bool TryResolveAllMatchingFunctions(
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        ICollection<IFunctionDescriptor> descriptors
    );
}