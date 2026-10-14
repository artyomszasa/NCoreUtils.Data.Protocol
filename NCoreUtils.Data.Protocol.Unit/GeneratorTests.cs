using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NCoreUtils.Data.Protocol.Unit;

public class GeneratorTests
{
    // GeneratedContext declares [ProtocolDescriptor(typeof(ExplicitComplexDataDescriptor))] (describing SomeComplexData)
    // and [ProtocolEntity(typeof(SomeComplexData))], so the explicit descriptor must be part of the generated context.

    [Fact]
    public void ExplicitDescriptorIsRegistered()
    {
        var descriptors = GeneratedContext.Singleton.GetTypeDescriptors().ToList();
        Assert.Contains(descriptors, d => d is ExplicitComplexDataDescriptor);
    }

    [Fact]
    public void ExplicitlyDescribedTypeHasDescriptor()
    {
        var descriptors = GeneratedContext.Singleton.GetTypeDescriptors().ToList();
        Assert.Contains(descriptors, d => d.Type == typeof(SomeComplexData));
    }

    // ItemWithComplexData has property of explicitly described type: generation used to fail with NCU0405.
    [Fact]
    public void ExplicitlyDescribedTypeCanBeUsedAsProperty()
    {
        var descriptors = GeneratedContext.Singleton.GetTypeDescriptors().ToList();
        var itemDescriptor = Assert.Single(descriptors, d => d.Type == typeof(ItemWithComplexData));
        Assert.Contains(itemDescriptor.Properties, p => p.Name == nameof(ItemWithComplexData.Data) && p.PropertyType == typeof(SomeComplexData));
        Assert.Contains(descriptors, d => d.Type == typeof(IEnumerable<SomeComplexData>));
    }
}
