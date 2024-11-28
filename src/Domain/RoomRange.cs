namespace Domain;

public record RoomRange(int? minimumRooms, int? maximumRooms)
{
    public bool IsInRange(int numberOfRooms)
    {
        return (!minimumRooms.HasValue || numberOfRooms >= minimumRooms.Value) &&
               (!maximumRooms.HasValue || numberOfRooms <= maximumRooms.Value);
    }
}