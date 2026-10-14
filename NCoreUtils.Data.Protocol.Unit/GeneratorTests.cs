using System;
using System.Collections.Generic;
using NCoreUtils.Data.Protocol.Internal;
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

    // Properties without accessible getter cannot be read by generated code: they used to break the build (CS0154, CS0271).
    [Fact]
    public void NonReadablePropertiesAreSkipped()
    {
        var descriptors = GeneratedContext.Singleton.GetTypeDescriptors().ToList();
        var descriptor = Assert.Single(descriptors, d => d.Type == typeof(ItemWithNonReadableProperties));
        var property = Assert.Single(descriptor.Properties);
        Assert.Equal(nameof(ItemWithNonReadableProperties.Id), property.Name);
    }

    private static ITypeDescriptor GetDescriptor<T>()
        => Assert.Single(GeneratedContext.Singleton.GetTypeDescriptors(), d => d.Type == typeof(T));

    // Explicit IParsable<T> implementation: emitted T.Parse(...) used to break the build.
    [Fact]
    public void ExplicitlyImplementedParsableIsParsed()
    {
        var descriptor = GetDescriptor<ExplicitlyParsable>();
        var value = Assert.IsType<ExplicitlyParsable>(descriptor.Parse("abc"));
        Assert.Equal("abc", value.Value);
        Assert.Equal("abc", descriptor.Stringify(value));
    }

    // Parse(string, IFormatProvider) inherited from base class used to be ignored --> type was not parseable.
    [Fact]
    public void InheritedParseIsUsed()
    {
        var descriptor = GetDescriptor<InheritedParsable>();
        var value = Assert.IsType<InheritedParsable>(descriptor.Parse("abc"));
        Assert.Equal("abc", value.Value);
        Assert.Equal("abc", descriptor.Stringify(value));
    }

    // Private Parse(string, IFormatProvider) used to be called from generated code --> build error.
    [Fact]
    public void InaccessibleParseIsIgnored()
    {
        var descriptor = GetDescriptor<PrivatelyParsable>();
        Assert.Throws<InvalidOperationException>(() => descriptor.Parse("abc"));
    }

    // Type detected as formattable without implementing IFormattable used to fail with InvalidCastException.
    [Fact]
    public void FormattableRequiresIFormattable()
    {
        var descriptor = GetDescriptor<FormattableWithoutInterface>();
        Assert.Throws<NotSupportedException>(() => descriptor.Stringify(new FormattableWithoutInterface("abc")));
    }
}
