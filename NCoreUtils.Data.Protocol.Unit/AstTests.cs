using System.Buffers;
using Xunit;

namespace NCoreUtils.Data.Protocol.Unit;

public class AstTests
{
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
    [InlineData("o => seed => every(o.sub, v => v.name = seed)")]
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