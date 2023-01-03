using System.Collections.Immutable;
using System.Diagnostics;
using NCoreUtils.Data.Protocol.Ast;

namespace NCoreUtils.Data.Protocol.Parsing;

public enum PrecedenceContext
{
    Or = 0,
    And = 1,
    Boolean = 2,
    PlusMinus = 3,
    MulDivRem = 4
}

public readonly ref struct ParserContext
{
    [DebuggerStepThrough]
    public static ParserContext Initial() => new(default, ImmutableDictionary<string, UniqueString>.Empty);

    private ImmutableDictionary<string, UniqueString> Parameters { get; }

    public PrecedenceContext Precedence { get; }

    public ParserContext(PrecedenceContext precedence, ImmutableDictionary<string, UniqueString> parameters)
    {
        Precedence = precedence;
        Parameters = parameters;
    }

    public UniqueString GetParameter(string name)
        => Parameters.TryGetValue(name, out var uname)
            ? uname
            : throw new ParserException($"Identifier {uname} is not valid in the current context.");

    [DebuggerStepThrough]
    public ParserContext AddParameter(string name, out UniqueString uname)
    {
        uname = new UniqueString(name);
        return new(Precedence, Parameters.Add(name, uname));
    }

    [DebuggerStepThrough]
    public ParserContext WithPrecedence(PrecedenceContext precedence)
        => new(precedence, Parameters);

#if DEBUG
    public override string ToString()
        => $"[{Precedence}, {string.Join(", ", Parameters.Values)}]";
#endif
}