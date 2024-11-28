using System.Text.RegularExpressions;

namespace Domain;

public class PostalCode
{
    public string Value { get; }

    public PostalCode(string value)
    {
        if (!Regex.IsMatch(value, @"^\d{5}$"))
            throw new InvalidPostalCodeException($"{value} is not a valid postal code");
        Value = value;
    }

    public bool IsInPostalCode(string propertyPostalCode)
    {
        return propertyPostalCode == Value;
    }
}