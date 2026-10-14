using Microsoft.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.Generator;

internal sealed class DiagnosticData(DiagnosticDescriptor descriptor, Location? location, object?[]? messageArgs)
{
    public DiagnosticDescriptor Descriptor { get; } = descriptor;

    public Location? Location { get; } = location;

    public object?[]? MessageArgs { get; } = messageArgs;

    public DiagnosticData WithLocation(Location? location)
        => new(Descriptor, location, MessageArgs);

    /// <summary>
    /// Diagnostic for unexpected exceptions: only the innermost frame is included to keep the message readable.
    /// </summary>
    public static DiagnosticData UnexpectedError(Exception exn, Location? location)
    {
        var frame = exn.StackTrace?.Split(['\n'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();
        return new(DiagnosticDescriptors.GenericError, location, [exn.GetType().Name, exn.Message, frame ?? "<unknown>"]);
    }
}