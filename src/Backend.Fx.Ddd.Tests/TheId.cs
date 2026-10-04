using Xunit;

namespace Backend.Fx.Ddd.Tests;

public class TheId
{
    [Fact]
    public void IdsOfSameTypeWithSameValueAreEqual()
    {
        var id1 = new CustomerId(1);
        var id2 = new CustomerId(1);

        Assert.Equal(id1, id2);
        Assert.True(id1 == id2);
        Assert.False(id1 != id2);
        Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
    }

    [Fact]
    public void IdsOfDifferentTypesWithSameValueAreNotEqual()
    {
        var customerId = new CustomerId(1);
        var orderId = new OrderId(1);

        Assert.False(customerId.Equals((object)orderId));
        Assert.False(orderId.Equals((object)customerId));
        Assert.NotEqual(customerId.GetHashCode(), orderId.GetHashCode());
    }

    [Fact]
    public void IdSatisfiesTheEquatableConstraintOfAggregateRoots()
    {
        // this only compiles when CustomerId implements IEquatable<CustomerId>
        var customer = new Customer(new CustomerId(1));
        Assert.Equal(new CustomerId(1), customer.Id);
    }

    [Fact]
    public void DefaultValueIsRejected()
    {
        Assert.Throws<ArgumentException>(() => new CustomerId(0));
        Assert.Throws<ArgumentException>(() => new Id(Guid.Empty));
    }

    [Fact]
    public void NegativeValueIsRejected()
    {
        Assert.Throws<ArgumentException>(() => new CustomerId(-1));
    }

    [Fact]
    public void ToStringStripsTheIdSuffix()
    {
        Assert.Equal("Customer/1", new CustomerId(1).ToString());
    }

    [Fact]
    public void ToStringKeepsTheNameOfATypeNamedId()
    {
        var value = Guid.NewGuid();
        Assert.Equal($"Id/{value}", new Id(value).ToString());
    }

    private class CustomerId(int value) : IntId<CustomerId>(value);

    private class OrderId(int value) : IntId<OrderId>(value);

    // a type that is literally named "Id" must not end up with an empty type name
    private class Id(Guid value) : GuidId<Id>(value);

    private class Customer(CustomerId id) : IAggregateRoot<CustomerId>
    {
        public CustomerId Id { get; } = id;
    }
}
