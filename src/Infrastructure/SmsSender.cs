using Domain;

namespace Infrastructure;

public interface ISmsSender
{
    void SendSMSAlert(SmsMessage message);
}

public class SmsSender : ISmsSender
{
    public void SendSMSAlert(SmsMessage message) {
        // Do not implement this code. Imagine the code is already here
    }
}