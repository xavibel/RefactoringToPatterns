namespace Domain;

public class Price
{
    public Price(int? minimumPrice)
    {
        if (minimumPrice.HasValue && minimumPrice < 0) 
            throw new InvalidPriceException("Price cannot be negative");
    }
}