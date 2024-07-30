using JetBrains.Annotations;
using Xunit;

namespace Backend.Fx.Ddd.Tests;

public class TheIdentified
{
    private readonly Sut _sut1 = new(1);
    private readonly Sut _sut2 = new(2);

    [Fact]
    public void IsNotEqualWhenIdIsDifferent()
    {
        Assert.False(_sut1.Equals(_sut2));
        Assert.False(_sut1 == _sut2);
    }

    [Fact]
    public void IsEqualWhenIdIsEqualButRefIsNotTheSame()
    {
        object sut1 = new Sut(_sut1.Id);
        Assert.True(sut1.Equals(_sut1));
    }

    [Fact]
    public void IsEqualWhenUsingOperator()
    {
        var sut1 = new Sut(_sut1.Id);
        Assert.True(sut1.Equals(_sut1));
        Assert.True(sut1 == _sut1);
    }

    [Fact]
    public void NullEqualsNull()
    {
        Sut? sut1 = null;
        Sut? sut2 = null;

        Assert.True(sut1 == sut2);
    }

    [Fact]
    public void HashCodeEqualsIdsHashCode()
    {
        Assert.Equal(_sut1.GetHashCode(), _sut1.Id.GetHashCode());
    }

    [PublicAPI]
    public class Sut(int id) : Identified<int>(id);
}
