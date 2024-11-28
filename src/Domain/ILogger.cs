namespace Domain;

public interface ILogger
{
    void Log(Dictionary<string, object> data);

    List<Dictionary<string, object>> GetLoggedData();
}