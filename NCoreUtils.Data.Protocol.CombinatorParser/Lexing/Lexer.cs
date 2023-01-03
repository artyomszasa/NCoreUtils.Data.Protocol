using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Source = NCoreUtils.Data.Protocol.IO.StringSource;

namespace NCoreUtils.Data.Protocol.Lexing;

public ref struct Lexer
{
    private ref struct Buffer
    {
        private char[] _buffer;

        private int _offset;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Buffer(int _)
        {
            _buffer = ArrayPool<char>.Shared.Rent(512);
            _offset = 0;
        }

        public void Push(char ch)
        {
            if (_offset >= _buffer.Length)
            {
                // resize
                char[] oldBuffer;
                (oldBuffer, _buffer) = (_buffer, ArrayPool<char>.Shared.Rent(_buffer.Length * 2));
                oldBuffer.AsSpan().CopyTo(_buffer.AsSpan());
                ArrayPool<char>.Shared.Return(oldBuffer);
            }
            _buffer[_offset++] = ch;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            ArrayPool<char>.Shared.Return(_buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => new(_buffer, 0, _offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Token HandleSingleCharToken(TokenType tokenType, ref Source source)
    {
        var position = source.Position;
        source.Advance(1);
        return new Token(tokenType, position, position + 1, default);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Token HandleDoubleCharToken(TokenType tokenType, ref Source source)
    {
        var position = source.Position;
        source.Advance(2);
        return new Token(tokenType, position, position + 2, default);
    }

    private static Token HandleNumLiteral(ReadOnlySpan<char> input, ref Source source)
    {
        var index = 1;
        // match first numeric block
        while (input.Length > index && CharUtils.IsDigit(input[index]))
        {
            ++index;
        }
        if (index != input.Length && input[index] == '.')
        {
            // match second numeric block
            for (++index; input.Length > index && CharUtils.IsDigit(input[index]); ++index) { }
        }
        var startPosition = source.Position;
        source.Advance(index);
        return Token.Num(startPosition, source.Position, new(input[..index]));
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="input">Input WITHOUT leading quote.</param>
    /// <param name="source">Character source.</param>
    /// <returns></returns>
    private static Token HandleStringLiteral(ReadOnlySpan<char> input, ref Source source)
    {
        if (input.Length == 0)
        {
            throw new InvalidOperationException($"Unclosed string literal at {source.Position}.");
        }
        var index = 0;
        var startPosition = source.Position;
        var prevCh = input[index];
        if (prevCh == '"')
        {
            source.Advance(2);
            return Token.String(startPosition, source.Position, string.Empty);
        }
        ++index;
        using var buffer = new Buffer(default);
        if (prevCh != '\\')
        {
            buffer.Push(prevCh);
        }
        foreach (var ch in input[1..])
        {
            if (prevCh == '\\')
            {
                switch (ch)
                {
                    case '\\':
                        buffer.Push('\\');
                        break;
                    case '"':
                        buffer.Push('"');
                        break;
                    case 'n':
                        buffer.Push('\n');
                        break;
                    case 'r':
                        buffer.Push('\r');
                        break;
                    case 't':
                        buffer.Push('\t');
                        break;
                    case 'v':
                        buffer.Push('\v');
                        break;
                    default:
                        buffer.Push('\\');
                        buffer.Push(ch);
                        break;
                }
            }
            else
            {
                switch (ch)
                {
                    case '\\':
                        break;
                    case '"':
                        source.Advance(index + 2);
                        return Token.String(startPosition, source.Position, buffer.ToString());
                    default:
                        buffer.Push(ch);
                        break;
                }
            }
            ++index;
            prevCh = ch;
        }
        throw new InvalidOperationException($"Unclosed string literal at {source.Position}.");
    }

    private static Token HandleIdent(ReadOnlySpan<char> input, ref Source source)
    {
        var index = 1;
        while (index < input.Length && CharUtils.IsLetterOrDigit(input[index]))
        {
            ++index;
        }
        var startPosition = source.Position;
        source.Advance(index);
        return Token.Ident(startPosition, source.Position, new(input[..index]));
    }

    private static Token HandleWhitespace(ReadOnlySpan<char> input, ref Source source)
    {
        var index = 1;
        while (index < input.Length && char.IsWhiteSpace(input[index]))
        {
            ++index;
        }
        var startPosition = source.Position;
        source.Advance(index);
        return Token.Ws(startPosition, source.Position, new(input[..index]));
    }

    private Source Source;

    public Lexer(string input)
        => Source = new(input);

    public Token Next()
    {
        if (Source.Eos)
        {
            return Token.Eos(Source.Position);
        }
        var input = Source.Pending;
        return input switch
        {
            ['.', ..] => HandleSingleCharToken(TokenType.Dot, ref Source),
            ['&', '&', ..] => HandleDoubleCharToken(TokenType.And, ref Source),
            ['|', '|', ..] => HandleDoubleCharToken(TokenType.Or, ref Source),
            ['=', '>', ..] => HandleDoubleCharToken(TokenType.Arrow, ref Source),
            ['=', ..] => HandleSingleCharToken(TokenType.Eq, ref Source),
            ['!', '=', ..] => HandleDoubleCharToken(TokenType.Neq, ref Source),
            ['<', '=', ..] => HandleDoubleCharToken(TokenType.Le, ref Source),
            ['>', '=', ..] => HandleDoubleCharToken(TokenType.Ge, ref Source),
            ['<', ..] => HandleSingleCharToken(TokenType.Lt, ref Source),
            ['>', ..] => HandleSingleCharToken(TokenType.Gt, ref Source),
            ['(', ..] => HandleSingleCharToken(TokenType.Lparen, ref Source),
            [')', ..] => HandleSingleCharToken(TokenType.Rparen, ref Source),
            [',', ..] => HandleSingleCharToken(TokenType.Comma, ref Source),
            ['+', ..] => HandleSingleCharToken(TokenType.Plus, ref Source),
            ['-', ..] => HandleSingleCharToken(TokenType.Minus, ref Source),
            ['/', ..] => HandleSingleCharToken(TokenType.Div, ref Source),
            ['*', ..] => HandleSingleCharToken(TokenType.Mul, ref Source),
            ['%', ..] => HandleSingleCharToken(TokenType.Mod, ref Source),
            ['"', .. var rest] => HandleStringLiteral(rest, ref Source),
            [var ch0, ..] when char.IsWhiteSpace(ch0) => HandleWhitespace(input, ref Source),
            [var ch0, ..] when CharUtils.IsDigit(ch0) => HandleNumLiteral(input, ref Source),
            [var ch0, ..] when CharUtils.IsLetter(ch0) => HandleIdent(input, ref Source),
            [var ch0, ..] => throw new InvalidOperationException($"Unexpected character '{ch0}' as {Source.Position}."),
            [] => Token.Eos(Source.Position)
        };
    }
}