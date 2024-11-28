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
        if (Regex.IsMatch(postalCode, "^\\d{5}$"))
        {
            if (!minimumPrice.HasValue || !(minimumPrice < 0))
            {
                if (!minimumPrice.HasValue || !maximumPrice.HasValue || !(minimumPrice > maximumPrice))
                {
                    string propertiesAsString = ReadPropertiesFile();
                    Property[] allProperties = JsonConvert.DeserializeObject<Property[]>(propertiesAsString);

                    var properties = allProperties
                        .Where(property => property.PostalCode == postalCode)
                        .Where(property => (!minimumPrice.HasValue || property.Price >= minimumPrice.Value) &&
                                           (!maximumPrice.HasValue || property.Price <= maximumPrice.Value))
                        .Where(property => (!minimumRooms.HasValue || property.NumberOfRooms >= minimumRooms.Value) &&
                                           (!maximumRooms.HasValue || property.NumberOfRooms <= maximumRooms.Value))
                        .Where(property =>
                            (!minimumSquareMeters.HasValue || property.SquareMeters >= minimumSquareMeters.Value) &&
                            (!maximumSquareMeters.HasValue || property.SquareMeters <= maximumSquareMeters.Value))
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

                throw new InvalidPriceException("The minimum price should be bigger than the maximum price");
            }

            throw new InvalidPriceException("Price cannot be negative");
        }

        throw new InvalidPostalCodeException($"{postalCode} is not a valid postal code");
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

public class InvalidPostalCodeException : Exception
{
    public InvalidPostalCodeException(string message) : base(message) { }
}

public class InvalidPriceException : Exception
{
    public InvalidPriceException(string message) : base(message) { }
}
