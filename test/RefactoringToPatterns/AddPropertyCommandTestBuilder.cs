namespace RefactoringToPatterns;

public class AddPropertyCommandTestBuilder
{
    private string postalCode = "04600";

    public AddPropertyCommand Build()
    {
        return new AddPropertyCommand(123, "New property", postalCode, 140_000, 3, 160, 1);
    }

    public AddPropertyCommandTestBuilder WithPostalCode(string postalCode)
    {
        this.postalCode = postalCode;
        return this;
    }
    
}