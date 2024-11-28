using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Domain;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace RefactoringToPatterns;

public class SearchProperty
{
    private readonly string propertiesFile;
    private readonly ILogger logger;
    private readonly bool addDateToLogger;

    public SearchProperty(string propertiesFile, ILogger logger, bool addDateToLogger)
    {
        this.propertiesFile = propertiesFile;
        this.logger = logger;
        this.addDateToLogger = addDateToLogger;
    }

    public Property[] Search(string postalCode, int? minimumPrice, int? maximumPrice, int? minimumRooms, int? maximumRooms, int? minimumSquareMeters, int? maximumSquareMeters)
    {
        var code = new PostalCode(postalCode);
        new Price(minimumPrice);
        
        string propertiesAsString = ReadPropertiesFile();
        Property[] allProperties = JsonConvert.DeserializeObject<Property[]>(propertiesAsString);

        var properties = allProperties
            .Where(property => code.IsInPostalCode(property.PostalCode))
            .Where(property => new PriceRange(new Price(minimumPrice), new Price(maximumPrice)).IsInRange(property.Price))
            .Where(property => new RoomRange(minimumRooms, maximumRooms).IsInRange(property.NumberOfRooms))
            .Where(property => new SquareMetersRange(minimumSquareMeters, maximumSquareMeters).IsInRange(property.SquareMeters))
            .ToArray();

        if (logger != null)
        {
            var data = new Dictionary<string, object>
            {
                { "postalCode", postalCode },
                { "minimumPrice", minimumPrice },
                { "maximumPrice", maximumPrice }
            };

            if (addDateToLogger)
            {
                data["date"] = DateTime.Now;
            }

            logger.Log(data);
        }

        return properties;

    }

    private string ReadPropertiesFile()
    {
        try
        {
            return File.ReadAllText(propertiesFile);
        }
        catch (IOException e)
        {
            throw new Exception("An error occurred while reading the properties file", e);
        }
    }
}