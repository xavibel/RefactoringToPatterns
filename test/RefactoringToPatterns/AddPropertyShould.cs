﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Domain;
using FluentAssertions;
using Infrastructure;
using Moq;
using Xunit;

namespace RefactoringToPatterns;

[Collection("Sequential")]
public class AddPropertyShould: IDisposable
{
    private static readonly string PROPERTIES = "resources/tmpAddPropertyProperties.json";
    private static readonly string ALERTS = "tmpTestAlerts.json";
    private static readonly int NON_EXISTING_OWNER = 999999;
    private static readonly string USERS_FILE = "resources/testUsers.json";

    [Fact]
    public void NewValidPropertyCanBeRetrieved()
    {
        var emailSenderMock = new Mock<EmailSender>();
        var addProperty = new AddProperty(PROPERTIES, USERS_FILE, emailSenderMock.Object, ALERTS,
            new Mock<SmsSender>().Object, new Mock<PushSender>().Object, null, false);

        addProperty.Execute(123, "New property", "04600", 140_000, 3, 160, 1);

        var propertiesAsString = File.ReadAllText(PROPERTIES);
        var allProperties = JsonSerializer.Deserialize<Property[]>(propertiesAsString);
        allProperties.Should().HaveCount(1);

        var property = allProperties[0];
        property.Id.Should().Be(123);
        property.Description.Should().Be("New property");
        property.PostalCode.Should().Be("04600");
        property.Price.Should().Be(140_000);
        property.NumberOfRooms.Should().Be(3);
        property.SquareMeters.Should().Be(160);
        property.OwnerId.Should().Be(1);
    }

    [Fact]
    public void CanStoreMoreThanOneProperty()
    {
        var addProperty = new AddProperty(PROPERTIES, USERS_FILE, new Mock<EmailSender>().Object, ALERTS,
            new Mock<SmsSender>().Object, new Mock<PushSender>().Object, null, false);
        addProperty.Execute(1, "New property", "04600", 140_000, 3, 160, 1);
        addProperty.Execute(2, "New property", "04600", 140_000, 3, 160, 1);

        var propertiesAsString = File.ReadAllText(PROPERTIES);
        var allProperties = JsonSerializer.Deserialize<Property[]>(propertiesAsString);
        allProperties.Should().HaveCount(2);
    }

    [Fact]
    public void FailsWhenThePostalCodeIsNotValid()
    {
        var addProperty = new AddProperty(PROPERTIES, USERS_FILE, new Mock<EmailSender>().Object, ALERTS,
            new Mock<SmsSender>().Object, new Mock<PushSender>().Object, null, false);

        Action act = () => addProperty.Execute(1, "New property", "046000", 140_000, 3, 160, 1);

        act.Should().Throw<InvalidPostalCodeException>().WithMessage("046000 is not a valid postal code");
    }

    [Fact]
    public void FailsWhenMinimumPriceIsNegative()
    {
        var addProperty = new AddProperty(PROPERTIES, USERS_FILE, new Mock<EmailSender>().Object, ALERTS,
            new Mock<SmsSender>().Object, new Mock<PushSender>().Object, null, false);

        Action act = () => addProperty.Execute(1, "New property", "04600", -1, 3, 160, 1);

        act.Should().Throw<InvalidPriceException>().WithMessage("Price cannot be negative");
    }

    [Fact]
    public void FailsWhenOwnerDoesNotExist()
    {
        var addProperty = new AddProperty(PROPERTIES, USERS_FILE, new Mock<EmailSender>().Object, ALERTS,
            new Mock<SmsSender>().Object, new Mock<PushSender>().Object, null, false);

        Action act = () => addProperty.Execute(1, "New property", "04600", 100_000, 3, 160, NON_EXISTING_OWNER);

        act.Should().Throw<InvalidUserIdException>().WithMessage($"The owner {NON_EXISTING_OWNER} does not exist");
    }

    [Fact]
    public void SendAlertByEmailWhenEmailAlert()
    {
        var alert = new Alert(2, "email", "04600", null, null, null, null, null, null);
        File.WriteAllText(ALERTS, JsonSerializer.Serialize(new List<Alert> { alert }));
        var emailSenderMock = new Mock<IEmailSender>();
        var addProperty = new AddProperty(PROPERTIES, USERS_FILE, emailSenderMock.Object, ALERTS,
            new Mock<SmsSender>().Object, new Mock<PushSender>().Object, null, false);

        addProperty.Execute(1, "New property", "04600", 100_000, 3, 160, 2);

        emailSenderMock.Verify(x => x.SendEmail(It.Is<Email>(e =>
            e.To == "rDeckard@email.com" &&
            e.Subject == "There is a new property at 04600" &&
            e.Body == "More information at https://properties.codium.team/1"
        )));
    }

    [Fact]
    public void SendAlertBySmsWhenSmsAlert()
    {
        var alert = new Alert(2, "sms", "04600", null, null, null, null, null, null);
        File.WriteAllText(ALERTS, JsonSerializer.Serialize(new List<Alert> { alert }));
        var smsSenderMock = new Mock<ISmsSender>();
        var addProperty = new AddProperty(PROPERTIES, USERS_FILE, new Mock<IEmailSender>().Object, ALERTS,
            smsSenderMock.Object, new Mock<IPushSender>().Object, null, false);

        addProperty.Execute(1, "New property", "04600", 100_000, 3, 160, 2);

        smsSenderMock.Verify(x => x.SendSMSAlert(It.Is<SmsMessage>(s =>
            s.PhoneNumber == "673777555" &&
            s.Message == "There is a new property at 04600. More information at https://properties.codium.team/1"
        )));
    }

    [Fact]
    public void SendAlertByPushWhenPushAlert()
    {
        var alert = new Alert(2, "push", "04600", null, null, null, null, null, null);
        File.WriteAllText(ALERTS, JsonSerializer.Serialize(new List<Alert> { alert }));
        var pushSenderMock = new Mock<IPushSender>();
        var addProperty = new AddProperty(PROPERTIES, USERS_FILE, new Mock<IEmailSender>().Object, ALERTS,
            new Mock<ISmsSender>().Object, pushSenderMock.Object, null, false);

        addProperty.Execute(1, "New property", "04600", 100_000, 3, 160, 2);

        pushSenderMock.Verify(x => x.SendPushNotification(It.Is<PushMessage>(p =>
            p.PhoneNumber == "673777555" &&
            p.Message == "There is a new property at 04600. More information at https://properties.codium.team/1"
        )));
    }

    [Fact]
    public void LogsTheRequestWhenThereIsALogger()
    {
        var loggerMock = new InMemoryLogger();
        var addProperty = new AddProperty(PROPERTIES, USERS_FILE, new Mock<EmailSender>().Object, ALERTS,
            new Mock<SmsSender>().Object, new Mock<PushSender>().Object, loggerMock, false);

        addProperty.Execute(1, "New property", "04600", 100_000, 3, 160, 2);

        loggerMock.GetLoggedData().Should().HaveCount(1);
        var log = loggerMock.GetLoggedData().First();
        log["id"].Should().Be(1);
        log["description"].Should().Be("New property");
        log["postalCode"].Should().Be("04600");
        log["price"].Should().Be(100_000);
        log["numberOfRooms"].Should().Be(3);
        log["squareMeters"].Should().Be(160);
        log["ownerId"].Should().Be(2);
    }

    [Fact]
    public void TheLoggedRequestContainsTheDateWhenRequired()
    {
        var loggerMock = new InMemoryLogger();
        var addProperty = new AddProperty(PROPERTIES, USERS_FILE, new Mock<EmailSender>().Object, ALERTS,
            new Mock<SmsSender>().Object, new Mock<PushSender>().Object, loggerMock, true);

        addProperty.Execute(1, "New property", "04600", 100_000, 3, 160, 2);

        var loggedData = loggerMock.GetLoggedData().First();
        loggedData.ContainsKey("date").Should().BeTrue();
    }

    [Fact]
    public void TheLoggedRequestDoesNotContainTheDateWhenNotRequired()
    {
        var loggerMock = new InMemoryLogger();
        var addProperty = new AddProperty(PROPERTIES, USERS_FILE, new Mock<EmailSender>().Object, ALERTS,
            new Mock<SmsSender>().Object, new Mock<PushSender>().Object, loggerMock, false);

        addProperty.Execute(1, "New property", "04600", 100_000, 3, 160, 2);

        var loggedData = loggerMock.GetLoggedData().First();
        loggedData.ContainsKey("date").Should().BeFalse();
    }

    public void Dispose()
    {
        try
        {
            File.Delete(PROPERTIES);
            File.Delete(ALERTS);
        }
        catch (IOException ignored)
        {
        }
    }

}