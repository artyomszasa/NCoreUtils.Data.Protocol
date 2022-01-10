using System;
using NCoreUtils.Data.Protocol.TypeInference;
using Xunit;

namespace NCoreUtils.Data.Protocol.Unit;

public class MiscTests
{
    [Fact]
    public void TypeUidTests()
    {
        var a = new TypeUid(1);
        var a1 = new TypeUid(1);
        var b = new TypeUid(2);
        Assert.Equal(a, a1);
        Assert.True(a == a1);
        Assert.False(a != a1);
        Assert.NotEqual(a, b);
        Assert.False(a == b);
        Assert.True(a != b);
        Assert.Equal("'1", a.ToString());
        Assert.Equal(a.ToString(), a1.ToString());
        Assert.True(((object)a).Equals(a1));
        Assert.False(((object)a).Equals(b));
        Assert.False(((object)a).Equals(1));
    }

    [Fact]
    public void TypeRefTests()
    {
        Assert.Equal(
            "type",
            Assert.Throws<ArgumentNullException>(() => new TypeRef(default!)).ParamName
        );
        var a = new TypeRef(typeof(int));
        var a1 = new TypeRef(typeof(int));
        var b = new TypeRef(typeof(string));
        Assert.Equal(a, a1);
        Assert.True(a == a1);
        Assert.False(a != a1);
        Assert.Equal(a.GetHashCode(), a1.GetHashCode());
        Assert.True(((object)a).Equals(a1));
        Assert.NotEqual(a, b);
        Assert.False(a == b);
        Assert.True(a != b);
        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
        Assert.False(((object)a).Equals(b));
        Assert.False(((object)a).Equals(2));
        Assert.False(((object)a).Equals(null));
    }
}