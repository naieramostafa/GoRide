using FluentAssertions;
using RideSharing.Core.ValueObjects;
using Xunit;

namespace RideSharing.UnitTests.Domain;

public class MoneyTests
{
    [Fact]
    public void CreateMoney_ShouldSetAmountAndCurrency()
    {
        var money = new Money(25.50m, "USD");

        money.Amount.Should().Be(25.50m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void CreateMoney_DefaultCurrency_ShouldBeUSD()
    {
        var money = new Money(10.00m);

        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Zero_ShouldReturnZeroAmount()
    {
        Money.Zero.Amount.Should().Be(0);
    }

    [Fact]
    public void Add_SameCurrency_ShouldSumAmounts()
    {
        var a = new Money(10.00m);
        var b = new Money(20.00m);

        var result = a + b;

        result.Amount.Should().Be(30.00m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_DifferentCurrency_ShouldThrow()
    {
        var a = new Money(10.00m, "USD");
        var b = new Money(20.00m, "EUR");

        Action act = () => { var _ = a + b; };
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Equality_SameValues_ShouldBeEqual()
    {
        var a = new Money(25.50m, "USD");
        var b = new Money(25.50m, "USD");

        (a == b).Should().BeTrue();
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentValues_ShouldNotBeEqual()
    {
        var a = new Money(25.50m);
        var b = new Money(30.00m);

        (a == b).Should().BeFalse();
    }
}
