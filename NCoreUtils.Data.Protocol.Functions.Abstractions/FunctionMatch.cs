using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace NCoreUtils.Data.Protocol;

/// <summary>
/// Represents output of the function matching.
/// </summary>
public struct FunctionMatch
{
    private readonly IReadOnlyList<Expression>? _arguments;

    /// <summary>
    /// Name of the protocol function.
    /// </summary>
    public string? Name { get; }

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

    public FunctionMatch(string name, IReadOnlyList<Expression> arguments)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
        }
        _arguments = arguments;
        Name = name;
    }
}