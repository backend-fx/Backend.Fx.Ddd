using System.Collections.Generic;
using JetBrains.Annotations;
using Xunit;

namespace Backend.Fx.Ddd.Tests;

public class TheValueObject
{
    private readonly Sut _sut1 = new("Value");
    private readonly Sut _sut2 = new("Value");
    private readonly Sut _sut3 = new("Another Value");
    private readonly Sut _sut4 = new(null);

    [Fact]
    public void ObjectIsNotEqualToNull()
    {
        Assert.False(_sut1.Equals(null));
        Assert.False(_sut1 == null);
        Assert.True(_sut1 != null);
    }

    [Fact]
    public void EquivalentObjectsHaveSameHashcode()
    {
        Assert.Equal(_sut1.GetHashCode(), _sut2.GetHashCode());
    }

    [Fact]
    public void DifferentObjectsHaveDifferentHashcode()
    {
        Assert.NotEqual(_sut1.GetHashCode(), _sut3.GetHashCode());
        Assert.NotEqual(_sut1.GetHashCode(), _sut4.GetHashCode());
    }

    [Fact]
    public void SameObjectIsEqual()
    {
        Assert.Equal(_sut1, _sut1);
        Assert.True(_sut1.Equals(_sut1));
#pragma warning disable CS1718 // Comparison made to same variable
        // ReSharper disable once EqualExpressionComparison
        Assert.True(_sut1 == _sut1);
        // ReSharper disable once EqualExpressionComparison
        Assert.False(_sut1 != _sut1);
#pragma warning restore CS1718
    }

    [Fact]
    public void EquivalentObjectIsEqual()
    {
        Assert.Equal(_sut1, _sut2);
        Assert.True(_sut1.Equals(_sut2));
        Assert.True(_sut1 == _sut2);
        Assert.False(_sut1 != _sut2);
    }

    [Fact]
    public void NonEquivalentObjectIsNotEqual()
    {
        Assert.NotEqual(_sut1, _sut3);
        Assert.False(_sut1.Equals(_sut3));
        Assert.False(_sut1 == _sut3);
        Assert.True(_sut1 != _sut3);
    }

    [Fact]
    public void NonEquivalentObjectIsNotEqualWhenPropertyIsNull()
    {
        Assert.NotEqual(_sut1, _sut4);
        Assert.False(_sut1.Equals(_sut4));
        Assert.False(_sut1 == _sut4);
        Assert.True(_sut1 != _sut4);
    }

    private class Sut(string? property) : ValueObject
    {
        [PublicAPI]
        public string? Property { get; } = property;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Property;
        }
    }
}
