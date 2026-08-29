namespace RideSharing.Core.ValueObjects;

public record Money(decimal Amount, string Currency = "USD")
{
    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Currency mismatch");
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money Zero => new(0);
}
