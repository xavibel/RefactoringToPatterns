namespace Domain;

public record Alert(
    int UserId,
    string AlertType,
    string PostalCode,
    int? MinimumPrice,
    int? MaximumPrice,
    int? MinimumRooms,
    int? MaximumRooms,
    int? MinimumSquareMeters,
    int? MaximumSquareMeters
);