using Domain;

namespace Infrastructure;

public interface IPushSender
{
    void SendPushNotification(PushMessage message);
}

public class PushSender : IPushSender
{
    public void SendPushNotification(PushMessage message) {
        // Do not implement this code. Imagine the code is already here

    }
}