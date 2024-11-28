using System.Text.RegularExpressions;

namespace Domain;

public class PostalCode
{
    public PostalCode(string postalCode)
    {
        if (!Regex.IsMatch(postalCode, @"^\d{5}$"))
            throw new InvalidPostalCodeException($"{postalCode} is not a valid postal code");
    }
}