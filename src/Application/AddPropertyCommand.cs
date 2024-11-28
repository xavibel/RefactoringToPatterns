namespace RefactoringToPatterns;

public record AddPropertyCommand(
    int Id,
    string Description,
    string PostalCode,
    int Price,
    int NumberOfRooms,
    int SquareMeters,
    int OwnerId);