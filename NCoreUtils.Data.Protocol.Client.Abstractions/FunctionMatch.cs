using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace NCoreUtils.Data.Protocol;

/// <summary>
/// Represents output of the function matching.
/// </summary>
public readonly struct FunctionMatch(string name, IReadOnlyList<Expression> arguments)
{
    private readonly IReadOnlyList<Expression>? _arguments = arguments;

    /// <summary>
    /// Name of the protocol function.
    /// </summary>
    public string? Name { get; } = name.ThrowIfNullOrWhiteSpace();

    /// <summary>
    /// Arguments to pass to the function call.
    /// </summary>
    public IReadOnlyList<Expression> Arguments
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _arguments ?? Array.Empty<Expression>();
    }

    /// <summary>
    /// <c>true</c> if function matching has been successfull, <c>false</c> otherwise.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Name))]
    public bool IsSuccess => !string.IsNullOrWhiteSpace(Name);
}