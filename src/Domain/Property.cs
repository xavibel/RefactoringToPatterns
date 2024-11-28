using System.Text.Json.Serialization;

namespace Domain;

public class Property
{
    public int Id  { get; set; }
    public string Description  { get; set; }
    public string PostalCode  { get; set; }
    public int Price { get; set; }
    public int NumberOfRooms { get; set; }
    public int SquareMeters { get; set; }
    public int OwnerId { get; set; }

    public Property(int Id, string Description, string PostalCode, int Price, int NumberOfRooms, int SquareMeters, int OwnerId)
    {
        this.Id = Id;
        this.Description = Description;
        this.PostalCode = PostalCode;
        this.Price = Price;
        this.NumberOfRooms = NumberOfRooms;
        this.SquareMeters = SquareMeters;
        this.OwnerId = OwnerId;
    }
}