namespace Domain;

public class Price
{
    public int? Value { get; }

    public Price(int? price)
    {
        if (price is < 0) 
            throw new InvalidPriceException("Price cannot be negative");

        Value = price;
    }
}