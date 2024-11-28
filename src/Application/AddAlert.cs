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

    public void Execute(AddAlertCommand addAlertCommand)
    {
        new PostalCode(addAlertCommand.PostalCode);
        new Price(addAlertCommand.MinimumPrice);
        PriceRange.FromInt(addAlertCommand.MinimumPrice, addAlertCommand.MaximumPrice);

        if (!IsAlertTypeValid(addAlertCommand.AlertType))
            throw new InvalidAlertTypeException($"The alert type {addAlertCommand.AlertType} does not exist");

        var usersAsString = ReadJsonFileContent(usersFile);
        var users = JsonConvert.DeserializeObject<List<User>>(usersAsString);
        var userExists = users.Any(user => user.Id == addAlertCommand.UserId);

        if (!userExists)
            throw new InvalidUserIdException($"The user {addAlertCommand.UserId} does not exist");

        var alerts = ReadAlerts();
        var alert = new Alert(addAlertCommand.UserId, addAlertCommand.AlertType, addAlertCommand.PostalCode, addAlertCommand.MinimumPrice, addAlertCommand.MaximumPrice,
            addAlertCommand.MinimumRooms,
            addAlertCommand.MaximumRooms,
            addAlertCommand.MinimumSquareMeters, addAlertCommand.MaximumSquareMeters);
        alerts.Add(alert);

        try
        {
            File.WriteAllText(alertsFile, JsonConvert.SerializeObject(alerts));
        }
        catch (IOException)
        {
        }

        if (logger != null)
        {
            var data = new Dictionary<string, object>
            {
                { "userId", addAlertCommand.UserId },
                { "alertType", addAlertCommand.AlertType },
                { "postalCode", addAlertCommand.PostalCode },
                { "minimumPrice", addAlertCommand.MinimumPrice },
                { "maximumPrice", addAlertCommand.MaximumPrice },
                { "minimumRooms", addAlertCommand.MinimumRooms },
                { "maximumRooms", addAlertCommand.MaximumRooms },
                { "minimumSquareMeters", addAlertCommand.MinimumSquareMeters },
                { "maximumSquareMeters", addAlertCommand.MaximumSquareMeters }
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