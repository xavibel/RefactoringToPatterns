namespace Domain;

public record RoomRange(int? minimumRooms, int? maximumRooms)
{
    public bool IsInRange(Property property)
    {
        return (!minimumRooms.HasValue || property.NumberOfRooms >= minimumRooms.Value) &&
               (!maximumRooms.HasValue || property.NumberOfRooms <= maximumRooms.Value);
    }
}