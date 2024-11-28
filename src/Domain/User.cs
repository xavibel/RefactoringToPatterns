namespace Domain;

public class User
{
    public int Id { get; }
    public string Name { get; }
    public string Email { get; }
    public string PhoneNumber { get; }

    public User(int id, string name, string email, string phoneNumber)
    {
        Id = id;
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
    }
}
