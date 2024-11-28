using System.Collections.Generic;
using Domain;

namespace Infrastructure;

public class InMemoryLogger : ILogger
{
    private readonly List<Dictionary<string, object>> loggedData = new();

    public void Log(Dictionary<string, object> data)
    {
        loggedData.Add(data);
    }

    public List<Dictionary<string, object>> GetLoggedData()
    {
        return loggedData;
    }
}