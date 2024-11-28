using Domain;

namespace Infrastructure;

public interface IEmailSender
{
    void SendEmail(Email email);
}

public class EmailSender : IEmailSender
{
    public void SendEmail(Email email) {
        // Do not implement this code. Imagine the code is already here
    }
}