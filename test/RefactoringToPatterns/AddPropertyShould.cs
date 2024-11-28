using System;
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
    private readonly IEmailSender emailSender;
    private readonly ISmsSender smsSender;
    private readonly IPushSender pushSender;
    private readonly AddPropertyCommandTestBuilder addPropertyCommandTestBuilder;

    public AddPropertyShould()
    {
        emailSender = new Mock<IEmailSender>().Object;
        smsSender = new Mock<ISmsSender>().Object;
        pushSender = new Mock<IPushSender>().Object;
        addPropertyCommandTestBuilder = new AddPropertyCommandTestBuilder();
    }
    [Fact]
    public void NewValidPropertyCanBeRetrieved()
    {
        var addProperty = NewAddProperty();

        addProperty.Execute(addPropertyCommandTestBuilder.WithId(123).Build());

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
        var addProperty = NewAddProperty();
        addProperty.Execute(addPropertyCommandTestBuilder.Build());
        addProperty.Execute(addPropertyCommandTestBuilder.Build());

        var propertiesAsString = File.ReadAllText(PROPERTIES);
        var allProperties = JsonSerializer.Deserialize<Property[]>(propertiesAsString);
        allProperties.Should().HaveCount(2);
    }

    [Fact]
    public void FailsWhenThePostalCodeIsNotValid()
    {
        var addProperty = NewAddProperty();

        Action act = () => addProperty.Execute(addPropertyCommandTestBuilder.WithPostalCode("046000").Build());

        act.Should().Throw<InvalidPostalCodeException>().WithMessage("046000 is not a valid postal code");
    }

    [Fact]
    public void FailsWhenMinimumPriceIsNegative()
    {
        var addProperty = NewAddProperty();

        Action act = () => addProperty.Execute(addPropertyCommandTestBuilder.WithPrice(-1).Build());

        act.Should().Throw<InvalidPriceException>().WithMessage("Price cannot be negative");
    }

    [Fact]
    public void FailsWhenOwnerDoesNotExist()
    {
        var addProperty = NewAddProperty();

        Action act = () => addProperty.Execute(addPropertyCommandTestBuilder.WithOwner(NON_EXISTING_OWNER).Build());

        act.Should().Throw<InvalidUserIdException>().WithMessage($"The owner {NON_EXISTING_OWNER} does not exist");
    }

    [Fact]
    public void SendAlertByEmailWhenEmailAlert()
    {
        var alert = new Alert(2, "email", "04600", null, null, null, null, null, null);
        File.WriteAllText(ALERTS, JsonSerializer.Serialize(new List<Alert> { alert }));
        var emailSenderMock = new Mock<IEmailSender>();
        var addProperty = NewAddPropertyWithEmailSender(emailSenderMock.Object);

        addProperty.Execute(addPropertyCommandTestBuilder.Build());

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
        var addProperty = NewAddPropertyWithSmsSender(smsSenderMock.Object);

        addProperty.Execute(addPropertyCommandTestBuilder.Build());

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
        var addProperty = NewAddPropertyWithPushSender(pushSenderMock.Object);

        addProperty.Execute(addPropertyCommandTestBuilder.Build());

        pushSenderMock.Verify(x => x.SendPushNotification(It.Is<PushMessage>(p =>
            p.PhoneNumber == "673777555" &&
            p.Message == "There is a new property at 04600. More information at https://properties.codium.team/1"
        )));
    }

    [Fact]
    public void LogsTheRequestWhenThereIsALogger()
    {
        var loggerMock = new InMemoryLogger();
        var addProperty = NewAddPropertyWithLogger(loggerMock, false);

        addProperty.Execute(addPropertyCommandTestBuilder.WithPrice(100000).WithOwner(2).Build());

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
        var addProperty = NewAddPropertyWithLogger(loggerMock, true);

        addProperty.Execute(addPropertyCommandTestBuilder.WithId(1).Build());

        var loggedData = loggerMock.GetLoggedData().First();
        loggedData.ContainsKey("date").Should().BeTrue();
    }

    [Fact]
    public void TheLoggedRequestDoesNotContainTheDateWhenNotRequired()
    {
        var loggerMock = new InMemoryLogger();
        var addProperty = NewAddPropertyWithLogger(loggerMock, false);

        addProperty.Execute(addPropertyCommandTestBuilder.Build());

        var loggedData = loggerMock.GetLoggedData().First();
        loggedData.ContainsKey("date").Should().BeFalse();
    }

    private AddProperty NewAddProperty()
    {
        return NewAddPropertyWithLogger(null, false);
    }

    private AddProperty NewAddPropertyWithSmsSender(ISmsSender sender)
    {
        return new AddProperty(PROPERTIES, USERS_FILE, emailSender, ALERTS,
            sender, pushSender, null, false);
    }
    
    private AddProperty NewAddPropertyWithPushSender(IPushSender sender)
    {
        return new AddProperty(PROPERTIES, USERS_FILE, emailSender, ALERTS,
            smsSender, sender, null, false);
    }
    
    private AddProperty NewAddPropertyWithEmailSender(IEmailSender sender)
    {
        return new AddProperty(PROPERTIES, USERS_FILE, sender, ALERTS,
            smsSender, pushSender, null, false);
    }
    private AddProperty NewAddPropertyWithLogger(InMemoryLogger? loggerMock, bool addDateToLogger)
    {
        return new AddProperty(PROPERTIES, USERS_FILE, emailSender, ALERTS,
            smsSender, pushSender, loggerMock, addDateToLogger);
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