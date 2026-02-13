using Microsoft.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.Generator;

internal static class DiagnosticDescriptors
{
    public static DiagnosticDescriptor GenericError = new(
        "NCU0000",
        "Generic Error",
        "{0}: {1} {2}",
        "ProtocolContextGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor ProcessingTarget = new(
        "NCU0100",
        "Processing target",
        "Processing target: {0}",
        "ProtocolContextGenerator",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor UnsupportedCSharpVersion = new(
        id: "NCU0101",
        title: "Unsupported C# version",
        messageFormat: "Unsupported C# version",
        category: "ProtocolContextGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor InvalidTargetSyntax = new(
        id: "NCU0102",
        title: "Invalid target syntax",
        messageFormat: "Invalid target syntax",
        category: "ProtocolContextGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor InvalidTargetNode = new(
        id: "NCU0103",
        title: "Invalid target node",
        messageFormat: "Invalid target node",
        category: "ProtocolContextGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor DoneCollectingTypes = new(
        "NCU0201",
        "Done collecting types",
        "Done collecting types [{0}ms]: {1}",
        "ProtocolContextGenerator",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor DoneEmittingContext = new(
        "NCU0202",
        "Done emitting context",
        "Done emitting context [{0}ms]: {1}",
        "ProtocolContextGenerator",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor NoDescribedTypeAttribute = new(
        id: "NCU0403",
        title: "Type descriptor must specify described type via attribute",
        messageFormat: "Type descriptor {0} must have DescribedType attribute dennoting the described type",
        category: "ProtocolContextGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor TypeNotFound = new(
        id: "NCU0404",
        title: "Element type not found",
        messageFormat: "Type {0} is required but has not been processed, for example is explicitly described",
        category: "ProtocolContextGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
}