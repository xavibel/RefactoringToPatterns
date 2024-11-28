using Domain;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json;

namespace RefactoringToPatterns;

[Collection("Sequential")]
public class AddAlertShould : IDisposable
{
    private const string ALERTS_FILE = "tmpTestAlerts.json";
    private const int NON_EXISTING_USER = 99999999;
    private const string USERS_FILE = @"resources\testUsers.json";

    [Fact]
    public void CanAddAnAlertWithAllTheSearchableFields()
    {
        var addAlert = new AddAlert(ALERTS_FILE, USERS_FILE, null, false);

        addAlert.Execute(1, "email", "08030", 0, 100_000, 0, 3, 30, 200);

        var content = File.ReadAllText(ALERTS_FILE);
        var alerts = JsonConvert.DeserializeObject<List<Alert>>(content);
        alerts.Should().HaveCount(1);

        var alert = alerts.First();
        alert.UserId.Should().Be(1);
        alert.AlertType.Should().Be("email");
        alert.PostalCode.Should().Be("08030");
        alert.MinimumPrice.Should().Be(0);
        alert.MaximumPrice.Should().Be(100_000);
        alert.MinimumRooms.Should().Be(0);
        alert.MaximumRooms.Should().Be(3);
        alert.MinimumSquareMeters.Should().Be(30);
        alert.MaximumSquareMeters.Should().Be(200);
    }

    [Fact]
    public void CanAddAnAlertOnlyWithPostalCode()
    {
        var addAlert = new AddAlert(ALERTS_FILE, USERS_FILE, null, false);

        addAlert.Execute(1, "email", "08030", null, null, null, null, null, null);

        var content = File.ReadAllText(ALERTS_FILE);
        var alerts = JsonConvert.DeserializeObject<List<Alert>>(content);
        alerts.Should().HaveCount(1);

        var alert = alerts.First();
        alert.MinimumPrice.Should().BeNull();
        alert.MaximumPrice.Should().BeNull();
        alert.MinimumRooms.Should().BeNull();
        alert.MaximumRooms.Should().BeNull();
        alert.MinimumSquareMeters.Should().BeNull();
        alert.MaximumSquareMeters.Should().BeNull();
    }

    [Fact]
    public void CanStoreMoreThanOneAlert()
    {
        var addAlert = new AddAlert(ALERTS_FILE, USERS_FILE, null, false);

        addAlert.Execute(1, "email", "08030", null, null, null, null, null, null);
        addAlert.Execute(1, "email", "08030", null, null, null, null, null, null);

        var content = File.ReadAllText(ALERTS_FILE);
        var alerts = JsonConvert.DeserializeObject<List<Alert>>(content);
        alerts.Should().HaveCount(2);
    }

    [Fact]
    public void FailsWhenThePostalCodeIsNotValid()
    {
        var addAlert = new AddAlert(ALERTS_FILE, USERS_FILE, null, false);

        Action action = () => addAlert.Execute(1, "email", "080300", null, null, null, null, null, null);

        action.Should().Throw<InvalidPostalCodeException>()
            .WithMessage("080300 is not a valid postal code");
    }

    [Fact]
    public void FailsWhenMinimumPriceIsNegative()
    {
        var addAlert = new AddAlert(ALERTS_FILE, USERS_FILE, null, false);

        Action action = () => addAlert.Execute(1, "email", "08030", -1, null, null, null, null, null);

        action.Should().Throw<InvalidPriceException>()
            .WithMessage("Price cannot be negative");
    }

    [Fact]
    public void FailsWhenMinimumPriceIsBiggerThanMaximumPrice()
    {
        var addAlert = new AddAlert(ALERTS_FILE, USERS_FILE, null, false);

        Action action = () => addAlert.Execute(1, "email", "08030", 100_001, 100_000, null, null, null, null);

        action.Should().Throw<InvalidPriceException>()
            .WithMessage("The minimum price should be bigger than the maximum price");
    }

    [Fact]
    public void FailsWhenTheUserDoesNotExist()
    {
        var addAlert = new AddAlert(ALERTS_FILE, USERS_FILE, null, false);

        Action action = () => addAlert.Execute(NON_EXISTING_USER, "email", "08030", null, null, null, null, null, null);

        action.Should().Throw<InvalidUserIdException>()
            .WithMessage($"The user {NON_EXISTING_USER} does not exist");
    }

    [Fact]
    public void FailsWhenTheNotificationTypeIsNotValid()
    {
        var addAlert = new AddAlert(ALERTS_FILE, USERS_FILE, null, false);

        Action action = () => addAlert.Execute(1, "asdf", "08030", null, null, null, null, null, null);

        action.Should().Throw<InvalidAlertTypeException>()
            .WithMessage("The alert type asdf does not exist");
    }

    [Theory]
    [InlineData("email")]
    [InlineData("sms")]
    [InlineData("push")]
    public void SucceedWithAnyAlertType(string alertType)
    {
        var addAlert = new AddAlert(ALERTS_FILE, USERS_FILE, null, false);

        addAlert.Execute(1, alertType, "08030", null, null, null, null, null, null);
    }

    [Fact]
    public void LogsTheRequest()
    {
        var logger = new InMemoryLogger();
        var addAlert = new AddAlert(ALERTS_FILE, USERS_FILE, logger, false);

        addAlert.Execute(1, "email", "08030", 0, 100_000, 0, 3, 30, 200);

        logger.GetLoggedData().Should().HaveCount(1);
        var loggedData = logger.GetLoggedData().First();
        loggedData["userId"].Should().Be(1);
        loggedData["alertType"].Should().Be("email");
        loggedData["postalCode"].Should().Be("08030");
        loggedData["minimumPrice"].Should().Be(0);
        loggedData["maximumPrice"].Should().Be(100_000);
        loggedData["minimumRooms"].Should().Be(0);
        loggedData["maximumRooms"].Should().Be(3);
        loggedData["minimumSquareMeters"].Should().Be(30);
        loggedData["maximumSquareMeters"].Should().Be(200);
    }

    [Fact]
    public void TheLoggedRequestContainsTheDateWhenRequired()
    {
        var logger = new InMemoryLogger();
        var addAlert = new AddAlert(ALERTS_FILE, USERS_FILE, logger, true);

        addAlert.Execute(1, "email", "08030", 0, 100_000, 0, 3, 30, 200);

        var loggedData = logger.GetLoggedData().First();
        loggedData.ContainsKey("date").Should().BeTrue();
    }

    [Fact]
    public void TheLoggedRequestDoesNotContainTheDateWhenNotRequired()
    {
        var logger = new InMemoryLogger();
        var addAlert = new AddAlert(ALERTS_FILE, USERS_FILE, logger, false);

        addAlert.Execute(1, "email", "08030", 0, 100_000, 0, 3, 30, 200);

        var loggedData = logger.GetLoggedData().First();
        loggedData.ContainsKey("date").Should().BeFalse();
    }


    public void Dispose()
    {
        if (File.Exists(ALERTS_FILE))
        {
            File.Delete(ALERTS_FILE);
        }
    }
}
