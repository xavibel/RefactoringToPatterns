using System.Collections.Generic;
using System.Linq;
using Domain;
using FluentAssertions;
using Infrastructure;
using Moq;
using Xunit;

namespace RefactoringToPatterns;

public class SearchPropertyTests
{
    private const string PROPERTIES = @"resources\testProperties.json"; // Ruta al archivo JSON de prueba.

    [Fact]
    public void FindPropertiesOfAPostalCode()
    {
        var searchProperty = new SearchProperty(PROPERTIES, null, false);

        var properties = searchProperty.Search("08030", null, null, null, null, null, null);

        properties.Should().HaveCount(1);
        properties[0].Description.Should().Be("Flat in Barcelona");
    }

    [Fact]
    public void FindPropertiesWithinAPriceRange()
    {
        var searchProperty = new SearchProperty(PROPERTIES, null, false);

        var properties = searchProperty.Search("04600", 10000, 100000, null, null, null, null);

        Assert.Single(properties);
        Assert.Equal("Cheap flat", properties[0].Description);
    }

    [Fact]
    public void FindPropertiesWithinRoomNumber()
    {
        var searchProperty = new SearchProperty(PROPERTIES, null, false);

        var properties = searchProperty.Search("04600", null, null, 1, 2, null, null);

        Assert.Single(properties);
        Assert.Equal("Cheap flat", properties[0].Description);
    }

    [Fact]
    public void FindPropertiesWithinSquareMeters()
    {
        var searchProperty = new SearchProperty(PROPERTIES, null, false);

        var properties = searchProperty.Search("04600", null, null, null, null, 80, 120);

        Assert.Single(properties);
        Assert.Equal("Cheap flat", properties[0].Description);
    }

    [Fact]
    public void FailsWhenPostalCodeIsNotValid()
    {
        var searchProperty = new SearchProperty(PROPERTIES, null, false);

        var exception = Assert.Throws<InvalidPostalCodeException>(() =>
            searchProperty.Search("046000", null, null, null, null, 0, 0));

        Assert.Equal("046000 is not a valid postal code", exception.Message);
    }

    [Fact]
    public void FailsWhenMinimumPriceIsNegative()
    {
        var searchProperty = new SearchProperty(PROPERTIES, null, false);

        var exception = Assert.Throws<InvalidPriceException>(() =>
            searchProperty.Search("04600", -1, null, null, null, 0, 0));

        Assert.Equal("Price cannot be negative", exception.Message);
    }

    [Fact]
    public void FailsWhenMinimumPriceIsBiggerThanMaximumPrice()
    {
        var searchProperty = new SearchProperty(PROPERTIES, null, false);

        var exception = Assert.Throws<InvalidPriceException>(() =>
            searchProperty.Search("04600", 100000, 99999, null, null, 0, 0));

        Assert.Equal("The minimum price should be bigger than the maximum price", exception.Message);
    }

    [Fact]
    public void LogsRequestWhenLoggerIsProvided()
    {
        var loggerMock = new Mock<ILogger>();
        var searchProperty = new SearchProperty(PROPERTIES, loggerMock.Object, false);

        searchProperty.Search("04600", 100000, 200000, 0, 999, null, null);

        loggerMock.Verify(logger => logger.Log(It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Fact]
    public void LoggedRequestContainsDateWhenRequired()
    {
        var logger = new InMemoryLogger();
        var searchProperty = new SearchProperty(PROPERTIES, logger, true);

        searchProperty.Search("04600", 100000, 200000, 0, 999, null, null);

        var loggedData = logger.GetLoggedData().First();
        Assert.True(loggedData.ContainsKey("date"));
    }

    [Fact]
    public void LoggedRequestDoesNotContainDateWhenNotRequired()
    {
        var logger = new InMemoryLogger();
        var searchProperty = new SearchProperty(PROPERTIES, logger, false);

        searchProperty.Search("04600", 100000, 200000, 0, 999, null, null);

        var loggedData = logger.GetLoggedData().First();
        Assert.False(loggedData.ContainsKey("date"));
    }
}

