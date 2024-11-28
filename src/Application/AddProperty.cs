using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Domain;
using Infrastructure;
using Newtonsoft.Json;

namespace RefactoringToPatterns;

public class AddProperty
{
    private readonly string propertiesFile;
    private readonly string usersFile;
    private readonly IEmailSender emailSender;
    private readonly string alertsFile;
    private readonly ISmsSender smsSender;
    private readonly IPushSender pushSender;
    private readonly ILogger logger;
    private readonly bool addDateToLogger;

    public AddProperty(string propertiesFile, string usersFile, IEmailSender emailSender, string alertsFile,
        ISmsSender smsSender, IPushSender pushSender, ILogger logger, bool addDateToLogger)
    {
        this.propertiesFile = propertiesFile;
        this.usersFile = usersFile;
        this.emailSender = emailSender;
        this.alertsFile = alertsFile;
        this.smsSender = smsSender;
        this.pushSender = pushSender;
        this.logger = logger;
        this.addDateToLogger = addDateToLogger;
    }

    public void Execute(int id, string description, string postalCode, int price, int numberOfRooms, int squareMeters,
        int ownerId)
    {
        if (!Regex.IsMatch(postalCode, @"^\d{5}$"))
            throw new InvalidPostalCodeException(postalCode + " is not a valid postal code");

        if (price < 0)
            throw new InvalidPriceException("Price cannot be negative");

        var usersAsString = ReadJSONFileContent(usersFile);
        var users = JsonConvert.DeserializeObject<List<User>>(usersAsString);
        
        var user = users.FirstOrDefault(u => u.Id == ownerId);
        if (user == null)
            throw new InvalidUserIdException("The owner " + ownerId + " does not exist");

        var propertiesAsString = ReadJSONFileContent(propertiesFile);
        var allProperties = JsonConvert.DeserializeObject<List<Property>>(propertiesAsString) ?? new List<Property>();
        var property = new Property(id, description, postalCode, price, numberOfRooms, squareMeters, ownerId);
        allProperties.Add(property);
        WritePropertiesFile(allProperties);

        string alertsAsString = ReadJSONFileContent(alertsFile);
        var alerts = JsonConvert.DeserializeObject<List<Alert>>(alertsAsString) ?? new List<Alert> { };

        foreach (var alert in alerts)
        {
            if (HasToSendTheAlert(property, alert))
            {
                var userToAlert = users.FirstOrDefault(u => u.Id == alert.UserId);
                if (userToAlert != null)
                {
                    if (alert.AlertType.Equals(AlertType.EMAIL.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        emailSender.SendEmail(new Email("noreply@codium.team", userToAlert.Email,
                            "There is a new property at " + property.PostalCode,
                            "More information at https://properties.codium.team/" + property.Id));
                    }
                    if (alert.AlertType.Equals(AlertType.SMS.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        smsSender.SendSMSAlert(new SmsMessage(userToAlert.PhoneNumber,
                            "There is a new property at " + property.PostalCode +
                            ". More information at https://properties.codium.team/" + property.Id));
                    }
                    if (alert.AlertType.Equals(AlertType.PUSH.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        pushSender.SendPushNotification(new PushMessage(userToAlert.PhoneNumber,
                            "There is a new property at " + property.PostalCode +
                            ". More information at https://properties.codium.team/" + property.Id));
                    }
                }
            }
        }

        if (logger != null)
        {
            var data = new Dictionary<string, object>
            {
                { "id", property.Id },
                { "description", property.Description },
                { "postalCode", property.PostalCode },
                { "price", property.Price },
                { "numberOfRooms", property.NumberOfRooms },
                { "squareMeters", property.SquareMeters },
                { "ownerId", property.OwnerId }
            };
            if (addDateToLogger)
            {
                data.Add("date", DateTime.Now.ToString("yyyy-MM-dd"));
            }

            logger.Log(data);
        }
    }

    private static bool HasToSendTheAlert(Property property, Alert alert)
    {
        return alert.PostalCode == property.PostalCode &&
               (alert.MinimumPrice == null || alert.MinimumPrice <= property.Price) &&
               (alert.MaximumPrice == null || alert.MaximumPrice >= property.Price) &&
               (alert.MinimumRooms == null || alert.MinimumRooms <= property.NumberOfRooms) &&
               (alert.MaximumRooms == null || alert.MaximumRooms >= property.NumberOfRooms) &&
               (alert.MinimumSquareMeters == null || alert.MinimumSquareMeters <= property.SquareMeters) &&
               (alert.MaximumSquareMeters == null || alert.MaximumSquareMeters >= property.SquareMeters);
    }

    private static string ReadJSONFileContent(string file)
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

    private void WritePropertiesFile(List<Property> allProperties)
    {
        try
        {
            File.WriteAllText(propertiesFile, JsonConvert.SerializeObject(allProperties));
        }
        catch (IOException ex)
        {
            throw new Exception("Error writing properties file", ex);
        }
    }
}