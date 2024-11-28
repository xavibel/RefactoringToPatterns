namespace Domain;

public record SquareMetersRange(int? minimumSquareMeters, int? maximumSquareMeters)
{
    public bool IsInRange(int numberOfRooms)
    {
        return (!minimumSquareMeters.HasValue || numberOfRooms >= minimumSquareMeters.Value) &&
               (!maximumSquareMeters.HasValue || numberOfRooms <= maximumSquareMeters.Value);
    }
}