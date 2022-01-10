using System;
using System.Buffers;
using NCoreUtils.Data.Protocol.Ast;
using Xunit;

namespace NCoreUtils.Data.Protocol.Unit;

public class AstTests
{
    [Fact]
    public void UniqueStringMisc()
    {
        var name = nameof(UniqueString);
        var a = new UniqueString(name);
        var b = new UniqueString(name);
        var c = new UniqueString("xxx");
        Assert.Equal(a, a);
        Assert.True(((object)a).Equals(a));
        Assert.NotEqual(a, b);
        Assert.NotEqual(a, c);
        Assert.NotEqual(a, default!);
        Assert.NotEqual(default!, b);
        Assert.False(((object)a).Equals(3));
#pragma warning disable CS1718
        Assert.True(a == a);
        Assert.False(a == b);
        Assert.False(a == c);
        Assert.False(a == default);
        Assert.False(default == a);
        Assert.False(default == default(UniqueString));
        Assert.False(a != a);
        Assert.True(a != b);
        Assert.True(a != c);
        Assert.True(a != default);
        Assert.True(default != a);
        Assert.True(default != default(UniqueString));
#pragma warning restore CS1718
        Assert.Equal(
            "value",
            Assert.Throws<ArgumentNullException>(() => new UniqueString(default!)).ParamName
        );
        Assert.Equal(a!.Value, b.Value);
        Assert.Equal(a!.Value, a.ToString());
    }

    [Theory]
    [InlineData("o => seed => every(o.sub, v => v.name = seed)", "x => y => every(x.sub, g => g.name = y)")]
    [InlineData("o => every(o.sub, v => v.name = \"2\")", "x => every(x.sub, g => g.name = 2)")]
    public void Equality(string rawA, string rawB)
    {
        var parser = new DefaultDataQueryParser();
        var a = parser.ParseQuery(rawA);
        var b = parser.ParseQuery(rawB);
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.True(((object)a).Equals(b));
    }

    [Theory]
    [InlineData("o => seed => every(o.sub, v => (v.name = seed) && ((v.name != \"xxx\") || (v.name = 2)))")]
    public void Stringification(string raw)
    {
        var parser = new DefaultDataQueryParser();
        var a = parser.ParseQuery(raw);
        var astr = a.ToString();
        Assert.Equal(raw, astr);
        var size = astr.Length;
        {
            using var memory = MemoryPool<char>.Shared.Rent(size);
            var used = a.Emplace(memory.Memory.Span);
            Assert.Equal(raw, memory.Memory.Span[..used].ToString());
        }
        {
            using var memory = MemoryPool<char>.Shared.Rent(size);
            Assert.True(a.TryEmplace(memory.Memory.Span, out var used));
            Assert.Equal(raw, memory.Memory.Span[..used].ToString());
        }
        {
            using var memory = MemoryPool<char>.Shared.Rent(size - 1);
            var exn = Assert.Throws<InsufficientBufferSizeException>(() => a.Emplace(memory.Memory.Span[..(size - 1)]));
            Assert.True(exn.SizeRequired.HasValue);
            Assert.Equal(size, exn.SizeRequired!.Value);
        }
    }
}