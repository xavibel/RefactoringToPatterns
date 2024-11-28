namespace RefactoringToPatterns;

public class AddPropertyCommandTestBuilder
{
    public AddPropertyCommand Build()
    {
        return new AddPropertyCommand(123, "New property", "04600", 140_000, 3, 160, 1);
    }
}