namespace NCoreUtils.Data.Protocol.Lexing;

public enum TokenType
{
    Ws,
    Dot,
    And,
    Or,
    Eq,
    Neq,
    Le,
    Ge,
    Lt,
    Gt,
    Lparen,
    Rparen,
    Comma,
    Arrow,
    Plus,
    Minus,
    Div,
    Mul,
    Mod,
    Num,
    String,
    Ident,
    Eos
}

public readonly struct Token
{
    public static Token Ws(int startPosition, int endPosition, string? value)
        => new(TokenType.Ws, startPosition, endPosition, value);

    public static Token Dot(int startPosition, int endPosition)
        => new(TokenType.Dot, startPosition, endPosition, string.Empty);

    public static Token And(int startPosition, int endPosition)
        => new(TokenType.And, startPosition, endPosition, string.Empty);

    public static Token Or(int startPosition, int endPosition)
        => new(TokenType.Or, startPosition, endPosition, string.Empty);

    public static Token Eq(int startPosition, int endPosition)
        => new(TokenType.Eq, startPosition, endPosition, string.Empty);

    public static Token Neq(int startPosition, int endPosition)
        => new(TokenType.Neq, startPosition, endPosition, string.Empty);

    public static Token Le(int startPosition, int endPosition)
        => new(TokenType.Le, startPosition, endPosition, string.Empty);

    public static Token Ge(int startPosition, int endPosition)
        => new(TokenType.Ge, startPosition, endPosition, string.Empty);

    public static Token Lt(int startPosition, int endPosition)
        => new(TokenType.Lt, startPosition, endPosition, string.Empty);

    public static Token Gt(int startPosition, int endPosition)
        => new(TokenType.Gt, startPosition, endPosition, string.Empty);

    public static Token Lparen(int startPosition, int endPosition)
        => new(TokenType.Lparen, startPosition, endPosition, string.Empty);

    public static Token Rparen(int startPosition, int endPosition)
        => new(TokenType.Rparen, startPosition, endPosition, string.Empty);

    public static Token Comma(int startPosition, int endPosition)
        => new(TokenType.Comma, startPosition, endPosition, string.Empty);

    public static Token Arrow(int startPosition, int endPosition)
        => new(TokenType.Arrow, startPosition, endPosition, string.Empty);

    public static Token Plus(int startPosition, int endPosition)
        => new(TokenType.Plus, startPosition, endPosition, string.Empty);

    public static Token Minus(int startPosition, int endPosition)
        => new(TokenType.Minus, startPosition, endPosition, string.Empty);

    public static Token Div(int startPosition, int endPosition)
        => new(TokenType.Div, startPosition, endPosition, string.Empty);

    public static Token Mul(int startPosition, int endPosition)
        => new(TokenType.Mul, startPosition, endPosition, string.Empty);

    public static Token Mod(int startPosition, int endPosition)
        => new(TokenType.Mod, startPosition, endPosition, string.Empty);

    public static Token Num(int startPosition, int endPosition, string value)
        => new(TokenType.Num, startPosition, endPosition, value);

    public static Token String(int startPosition, int endPosition, string? value)
        => new(TokenType.String, startPosition, endPosition, value);

    public static Token Ident(int startPosition, int endPosition, string? value)
        => new(TokenType.Ident, startPosition, endPosition, value);

    public static Token Eos(int position)
        => new(TokenType.Eos, position, position, string.Empty);

    public TokenType TokenType { get; }

    public int StartPosition { get; }

    public int EndPosition { get; }

    public string? Value { get; }

    internal Token(TokenType tokenType, int startPosition, int endPosition, string? value)
    {
        TokenType = tokenType;
        StartPosition = startPosition;
        EndPosition = endPosition;
        Value = value;
    }

    public override string ToString() => TokenType switch
    {
        TokenType.Ws => "Ws",
        TokenType.Dot => "Dot",
        TokenType.And => "And",
        TokenType.Or => "Or",
        TokenType.Eq => "Eq",
        TokenType.Neq => "Neq",
        TokenType.Le => "Le",
        TokenType.Ge => "Ge",
        TokenType.Lt => "Lt",
        TokenType.Gt => "Gt",
        TokenType.Lparen => "Lparen",
        TokenType.Rparen => "Rparen",
        TokenType.Comma => "Comma",
        TokenType.Arrow => "Arrow",
        TokenType.Plus => "Plus",
        TokenType.Minus => "Minus",
        TokenType.Div => "Div",
        TokenType.Mul => "Mul",
        TokenType.Mod => "Mod",
        TokenType.Num => $"Num[{Value}]",
        TokenType.String => $"String[{Value}]",
        TokenType.Ident => $"Ident[{Value}]",
        TokenType.Eos => "Eos",
        _ => "<<invalid>>"
    };
}

