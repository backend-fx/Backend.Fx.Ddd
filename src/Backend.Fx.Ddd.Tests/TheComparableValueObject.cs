using System;
using System.Collections.Generic;
using Xunit;

namespace Backend.Fx.Ddd.Tests;

public class TheComparableValueObject
{
    private readonly Sut _sut1 = new Sut("Aaaa");
    private readonly Sut _sut1B = new Sut("Aaaa");
    private readonly Sut _sut2 = new Sut("Bbbb");

    [Fact]
    public void CanCompare()
    {
        Assert.Equal(-1, _sut1.CompareTo(_sut2));
        Assert.Equal(1, _sut2.CompareTo(_sut1));
        Assert.Equal(0, _sut1.CompareTo(_sut1));
        Assert.Equal(0, _sut1.CompareTo(_sut1B));
        Assert.Equal(1, _sut1.CompareTo(null));
    }

    private class Sut(string property) : ComparableValueObject
    {
        public string Property { get; } = property;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Property;
        }

        protected override IEnumerable<IComparable> GetComparableComponents()
        {
            yield return Property;
        }
    }
}
