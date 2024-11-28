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

    public Property[] Search(SearchPropertyQuery searchPropertyQuery)
    {
        var code = new PostalCode(searchPropertyQuery.PostalCode);
        new Price(searchPropertyQuery.MinimumPrice);
        
        string propertiesAsString = ReadPropertiesFile();
        Property[] allProperties = JsonConvert.DeserializeObject<Property[]>(propertiesAsString);

        var properties = allProperties
            .Where(property => code.IsInPostalCode(property.PostalCode))
            .Where(property => PriceRange.FromInt(searchPropertyQuery.MinimumPrice, searchPropertyQuery.MaximumPrice).IsInRange(property.Price))
            .Where(property => new RoomRange(searchPropertyQuery.MinimumRooms, searchPropertyQuery.MaximumRooms).IsInRange(property.NumberOfRooms))
            .Where(property => new SquareMetersRange(searchPropertyQuery.MinimumSquareMeters, searchPropertyQuery.MaximumSquareMeters).IsInRange(property.SquareMeters))
            .ToArray();

        if (logger != null)
        {
            var data = new Dictionary<string, object>
            {
                { "postalCode", searchPropertyQuery.PostalCode },
                { "minimumPrice", searchPropertyQuery.MinimumPrice },
                { "maximumPrice", searchPropertyQuery.MaximumPrice }
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