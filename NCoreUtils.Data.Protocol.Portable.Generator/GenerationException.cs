using System;
using Microsoft.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.Generator;

internal class GenerationException(DiagnosticData diagnosticData) : InvalidOperationException
{
    public DiagnosticData DiagnosticData { get; } = diagnosticData ?? throw new ArgumentNullException(nameof(diagnosticData));
}

internal class NoDescribedTypeException(string descriptorType, Location? location)
    : GenerationException(CreateDiagnosticData(descriptorType, location))
{
    private static DiagnosticData CreateDiagnosticData(string descriptorType, Location? location) => new(
        DiagnosticDescriptors.NoDescribedTypeAttribute,
        location,
        [descriptorType]
    );
}