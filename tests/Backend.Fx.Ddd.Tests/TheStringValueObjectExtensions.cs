using System;
using Backend.Fx.Exceptions;
using Xunit;

namespace Backend.Fx.Ddd.Tests;

public class TheStringValueObjectExtensions
{
    [Fact]
    public void CanCreateMandatoryStringValue()
    {
        var sut = "a".AsMandatoryValue<MyStringValue>();
        Assert.Equal("a", sut.Value);
    }

    [Fact]
    public void CannotCreateMandatoryStringValueFromEmpty()
    {
        Assert.Throws<ArgumentNullException>(() => "".AsMandatoryValue<MyStringValue>());
    }

    [Fact]
    public void CannotCreateMandatoryStringValueFromEmptyAndReportsError()
    {
        var exceptionBuilder = new ExceptionBuilder<ClientException>();
        "".AsMandatoryValue<MyStringValue>(exceptionBuilder);

        try
        {
            exceptionBuilder.Dispose();
        }
        catch (ClientException ex)
        {
            Assert.Single(ex.Errors);
        }
    }

    [Fact]
    public void CanCreateMandatoryStringValueFromNull()
    {
        string? nullString = null;
        Assert.Throws<ArgumentNullException>(() => nullString.AsMandatoryValue<MyStringValue>());
    }

    [Fact]
    public void CanCreateOptionalStringValue()
    {
        var sut = "a".AsOptionalValue<MyStringValue>();
        Assert.Equal("a", sut.Value);
    }

    [Fact]
    public void CanCreateOptionalStringValueFromEmpty()
    {
        var sut = "".AsOptionalValue<MyStringValue>();
        Assert.True(sut.IsEmpty);
    }

    [Fact]
    public void CanCreateOptionalStringValueFromNull()
    {
        string? nullString = null;
        var sut = nullString.AsOptionalValue<MyStringValue>();
        Assert.True(sut.IsEmpty);
    }

    [Fact]
    public void CanCreateEmptyStringValue()
    {
        var sut = StringValueObject.Empty<MyStringValue>();
        Assert.True(sut.IsEmpty);
    }
}
