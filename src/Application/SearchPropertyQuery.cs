namespace RefactoringToPatterns;

public record SearchPropertyQuery(
    string PostalCode,
    int? MinimumPrice,
    int? MaximumPrice,
    int? MinimumRooms,
    int? MaximumRooms,
    int? MinimumSquareMeters,
    int? MaximumSquareMeters);