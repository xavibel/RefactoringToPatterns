namespace RefactoringToPatterns;

public class AddPropertyCommandTestBuilder
{
    private string postalCode = "04600";
    private int price = 140_000;
    private int squareMeters = 160;
    private int numberOfRooms = 3;
    private int ownerId = 1;
    private int id = 1;

    public AddPropertyCommand Build()
    {
        return new AddPropertyCommand(id, "New property", postalCode, price, numberOfRooms, squareMeters, ownerId);
    }

    public AddPropertyCommandTestBuilder WithId(int id)
    {
        this.id = id;
        return this;
    }
    public AddPropertyCommandTestBuilder WithPostalCode(string postalCode)
    {
        this.postalCode = postalCode;
        return this;
    }
    public AddPropertyCommandTestBuilder WithPrice(int price)
    {
        this.price = price;
        return this;
    }
    
    public AddPropertyCommandTestBuilder WithNumberOfRooms(int numberOfRooms)
    {
        this.numberOfRooms = numberOfRooms;
        return this;
    }
    public AddPropertyCommandTestBuilder WithSquareMeters(int squareMeters)
    {
        this.squareMeters = squareMeters;
        return this;
    }
    
    public AddPropertyCommandTestBuilder WithOwner(int ownerId)
    {
        this.ownerId = ownerId;
        return this;
    }
    
    
}