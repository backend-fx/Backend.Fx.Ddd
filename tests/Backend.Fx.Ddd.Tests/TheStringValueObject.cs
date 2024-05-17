using System;
using Xunit;

namespace Backend.Fx.Ddd.Tests;

public class TheStringValueObject
{
    [Fact]
    public void CannotBeCreatedWithNullString()
    {
        Assert.Throws<ArgumentNullException>(() => new MyStringValue(null!));
    }

    [Fact]
    public void CannotBeCreatedWithNegativeMinLength()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConfigurableString("a", -1));
    }

    [Fact]
    public void CannotBeCreatedWithNegativeMaxLength()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConfigurableString("a", maxLength: -1));
    }

    [Fact]
    public void CannotBeCreatedWithMaxLengthSmallerThanMinLength()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConfigurableString("a", 1, 0));
    }

    [Fact]
    public void CanBeCreatedFromEmptyString()
    {
        var sut = new MyStringValue(string.Empty);
        Assert.Equal(string.Empty, sut.Value);
    }

    [Theory]
    [InlineData(" a ")]
    [InlineData("a ")]
    [InlineData(" a")]
    public void IsTrimmedOnCreation(string s)
    {
        var sut = new MyStringValue(s);
        Assert.Equal("a", sut.Value);
    }

    [Fact]
    public void CannotBeCreatedWithShorterValueThanMinLength()
    {
        Assert.Throws<ArgumentException>(() => new ConfigurableString("a", 2));
    }

    [Fact]
    public void CannotBeCreatedWithLongerValueThanMaxLength()
    {
        Assert.Throws<ArgumentException>(() => new ConfigurableString("aa", maxLength: 1));
    }

    [Fact]
    public void CanBeCreatedWithCorrectLength()
    {
        var sut = new ConfigurableString("a", 1, 1);
        Assert.Equal("a", sut.Value);
    }

    [Fact]
    public void CanBeCreatedWithMaxLength()
    {
        var sut = new ConfigurableString("a", maxLength: 1);
        Assert.Equal("a", sut.Value);
    }

    [Fact]
    public void CanBeCreatedWithMinLength()
    {
        var sut = new ConfigurableString("a", 1);
        Assert.Equal("a", sut.Value);
    }

    [Fact]
    public void DetectsIsEmpty()
    {
        var sut = new MyStringValue(string.Empty);
        Assert.True(sut.IsEmpty);

        sut = new MyStringValue("not empty");
        Assert.False(sut.IsEmpty);
    }

    [Fact]
    public void CanCheckEquality()
    {
        var sut1 = new MyStringValue("a");
        var sut2 = new MyStringValue("a");
        Assert.Equal(sut1, sut2);

        sut2 = new MyStringValue("b");
        Assert.NotEqual(sut1, sut2);
    }

    [Fact]
    public void CanCheckEqualityWithNull()
    {
        var sut1 = new MyStringValue("a");
        Assert.False(sut1 == null);
    }

    [Fact]
    public void CanCheckEqualityWithObject()
    {
        var sut1 = new MyStringValue("a");
        Assert.False(sut1.Equals(new object()));
    }

    [Fact]
    public void CanCheckEqualityWithSameReference()
    {
        var sut1 = new MyStringValue("a");
        var sut2 = sut1;
        Assert.True(sut1 == sut2);
    }

    [Fact]
    public void CanCheckEqualityWithDifferentType()
    {
        var sut1 = new MyStringValue("a");
        var sut2 = new AnotherStringValue("a");
        Assert.False(sut1 == sut2);
    }

    [Fact]
    public void CanCompare()
    {
        var sut1 = new MyStringValue("a");
        var sut2 = new MyStringValue("a");
        var sut3 = new MyStringValue("b");
        Assert.Equal(0, sut1.CompareTo(sut2));
        Assert.Equal(-1, sut1.CompareTo(sut3));
        Assert.Equal(1, sut3.CompareTo(sut1));
    }

    [Fact]
    public void CanBeImplicitlyCastedToString()
    {
        var sut = new MyStringValue("a");
        Assert.Equal("a", sut);
    }

    [Fact]
    public void ToStringIsAnImplicitCastToString()
    {
        var sut = new MyStringValue("a");
        Assert.Equal("a", sut.ToString());
    }
}

public class MyStringValue(string value)
    : StringValueObject(value);


public class ConfigurableString(string value, int minLength = 0, int maxLength = int.MaxValue)
    : StringValueObject(value, minLength, maxLength);

public class AnotherStringValue(string value)
    : StringValueObject(value);
