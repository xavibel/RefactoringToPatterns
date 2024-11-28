namespace Domain;

public class PriceRange
{
    public Price MinPrice { get; }
    public Price MaxPrice { get; }

    public PriceRange(Price minPrice, Price maxPrice)
    {
        MinPrice = minPrice;
        MaxPrice = maxPrice;
        if (minPrice.Value > maxPrice.Value)
            throw new InvalidPriceException("The minimum price should be bigger than the maximum price");
    }

    public bool IsInRange(int propertyPrice) =>
        (!MinPrice.Value.HasValue || propertyPrice >= MinPrice.Value) &&
        (!MaxPrice.Value.HasValue || propertyPrice <= MaxPrice.Value);
}