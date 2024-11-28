using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Domain;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace RefactoringToPatterns;

public class AddAlert
{
    private readonly string alertsFile;
    private readonly string usersFile;
    private readonly ILogger logger;
    private readonly bool addDateToLogger;

    public AddAlert(string alertsFile, string usersFile, ILogger logger, bool addDateToLogger)
    {
        this.alertsFile = alertsFile;
        this.usersFile = usersFile;
        this.logger = logger;
        this.addDateToLogger = addDateToLogger;
    }

    private static string ReadJsonFileContent(string file)
    {
        try
        {
            return File.ReadAllText(file);
        }
        catch (IOException)
        {
            return "[]";
        }
    }

    private static bool IsAlertTypeValid(string alertType)
    {
        return Enum.TryParse(typeof(AlertType), alertType.ToUpper(), out _);
    }

    public void Execute(
        int userId,
        string alertType,
        string postalCode,
        int? minimumPrice,
        int? maximumPrice,
        int? minimumRooms,
        int? maximumRooms,
        int? minimumSquareMeters,
        int? maximumSquareMeters)
    {
        new PostalCode(postalCode);
        
        if (minimumPrice < 0)
            throw new InvalidPriceException("Price cannot be negative");
        
        if (minimumPrice.HasValue && maximumPrice.HasValue && minimumPrice > maximumPrice)
            throw new InvalidPriceException("The minimum price should be bigger than the maximum price");
        
        if (!IsAlertTypeValid(alertType))
            throw new InvalidAlertTypeException($"The alert type {alertType} does not exist");

        var usersAsString = ReadJsonFileContent(usersFile);
        var users = JsonConvert.DeserializeObject<List<User>>(usersAsString);
        var userExists = users.Any(user => user.Id == userId);

        if (!userExists)
            throw new InvalidUserIdException($"The user {userId} does not exist");
        
        var alerts = ReadAlerts();
        var alert = new Alert(userId, alertType, postalCode, minimumPrice, maximumPrice,
            minimumRooms,
            maximumRooms,
            minimumSquareMeters, maximumSquareMeters);
        alerts.Add(alert);

        try
        {
            File.WriteAllText(alertsFile, JsonConvert.SerializeObject(alerts));
        }
        catch (IOException) { }

        if (logger != null)
        {
            var data = new Dictionary<string, object>
            {
                { "userId", userId },
                { "alertType", alertType },
                { "postalCode", postalCode },
                { "minimumPrice", minimumPrice },
                { "maximumPrice", maximumPrice },
                { "minimumRooms", minimumRooms },
                { "maximumRooms", maximumRooms },
                { "minimumSquareMeters", minimumSquareMeters },
                { "maximumSquareMeters", maximumSquareMeters }
            };

            if (addDateToLogger)
            {
                data["date"] = DateTime.Now;
            }

            logger.Log(data);
        }
    }

    private List<Alert> ReadAlerts()
        {
            var content = ReadJsonFileContent(alertsFile);
            return JsonConvert.DeserializeObject<List<Alert>>(content) ?? new List<Alert>();
        }
    }