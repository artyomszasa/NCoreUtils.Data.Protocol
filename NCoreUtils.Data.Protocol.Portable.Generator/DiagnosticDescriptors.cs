using Microsoft.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.Generator;

internal static class DiagnosticDescriptors
{
    public static DiagnosticDescriptor ProcessingTarget = new(
        "NCU0100",
        "Processing target",
        "Processing target: {0}",
        "ProtocolContextGenerator",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor DoneCollectingTypes = new(
        "NCU0101",
        "Done collecting types",
        "Done collecting types [{0}ms]: {1}",
        "ProtocolContextGenerator",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor DoneEmittingContext = new(
        "NCU0102",
        "Done emitting context",
        "Done emitting context [{0}ms]: {1}",
        "ProtocolContextGenerator",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true
    );
}