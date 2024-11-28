namespace Domain;

public class PriceRange
{
    public Price MinPrice { get; }
    public Price MaxPrice { get; }

    private PriceRange(Price minPrice, Price maxPrice)
    {
        MinPrice = minPrice;
        MaxPrice = maxPrice;
        if (minPrice.Value > maxPrice.Value)
            throw new InvalidPriceException("The minimum price should be bigger than the maximum price");
    }

    public static PriceRange FromInt(int? minPrice, int? maxPrice)
    {
        return new PriceRange(new Price(minPrice), new Price(maxPrice));
    }


    public bool IsInRange(int propertyPrice) =>
        (!MinPrice.Value.HasValue || propertyPrice >= MinPrice.Value) &&
        (!MaxPrice.Value.HasValue || propertyPrice <= MaxPrice.Value);
}