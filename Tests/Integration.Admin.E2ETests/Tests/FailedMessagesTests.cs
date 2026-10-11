using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using Integration.Admin.E2ETests.Configuration;
using Integration.Admin.E2ETests.Fixtures;
using Integration.Admin.E2ETests.Pages;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace Integration.Admin.E2ETests.Tests;

public sealed class FailedMessagesTests : IClassFixture<TestEnvironmentFixture>
{
    public FailedMessagesTests(TestEnvironmentFixture fixture)
    {
    }

    [Fact]
    public void AdminHomePage_LoadsSuccessfully()
    {
        var options = new ChromeOptions();

        using var driver = new RemoteWebDriver(
            new Uri(E2ETestSettings.SeleniumUrl),
            options.ToCapabilities());

        var page = new FailedMessagesPage(driver);

        page.Navigate();

        page.IsLoaded().Should().BeTrue();
    }

    [Fact]
    public void FailedMessagesPage_DisplaysSeededMessages()
    {
        var options = new ChromeOptions();

        using var driver = new RemoteWebDriver(
            new Uri(E2ETestSettings.SeleniumUrl),
            options.ToCapabilities());

        var page = new FailedMessagesPage(driver);

        page.Navigate();

        page.IsLoaded().Should().BeTrue(
            "the Failed Messages page should load before checking its data");

        var sourceIds = page.GetSourceIds();

        sourceIds.Should().Contain(
            "E2E-TRANSIENT-001",
            "E2E-PERMANENT-001",
            "E2E-CORRECT-PAYLOAD-001");
    }


    [Fact]
    public void FailedMessageDetails_DisplaysCorrectMessageAndPayload()
    {
        var options = new ChromeOptions();

        using var driver = new RemoteWebDriver(
            new Uri(E2ETestSettings.SeleniumUrl),
            options.ToCapabilities());

        var messagesPage = new FailedMessagesPage(driver);
        messagesPage.Navigate();

        messagesPage.IsLoaded().Should().BeTrue();

        messagesPage.OpenDetails("E2E-CORRECT-PAYLOAD-001");

        var detailsPage = new FailedMessageDetailsPage(driver);

        detailsPage.IsLoaded().Should().BeTrue();
        detailsPage.GetSourceId()
            .Should()
            .Be("E2E-CORRECT-PAYLOAD-001");

        detailsPage.IsReplayButtonDisplayed().Should().BeTrue();

        using var payload = JsonDocument.Parse(detailsPage.GetPayload());

        payload.RootElement
            .GetProperty("data")
            .GetProperty("customerId")
            .GetInt32()
            .Should()
            .Be(-1);
    }

    [Fact]
    public void FailedMessageDetails_CorrectsPayloadAndReplaysMessage()
    {
        var options = new ChromeOptions();

        using var driver = new RemoteWebDriver(
            new Uri(E2ETestSettings.SeleniumUrl),
            options.ToCapabilities());

        var messagesPage = new FailedMessagesPage(driver);
        messagesPage.Navigate();

        messagesPage.IsLoaded().Should().BeTrue();

        messagesPage.OpenDetails("E2E-CORRECT-PAYLOAD-001");

        var detailsPage = new FailedMessageDetailsPage(driver);

        detailsPage.IsLoaded().Should().BeTrue();
        detailsPage.GetSourceId()
            .Should()
            .Be("E2E-CORRECT-PAYLOAD-001");

        using (var originalPayload = JsonDocument.Parse(detailsPage.GetPayload()))
        {
            originalPayload.RootElement
                .GetProperty("data")
                .GetProperty("customerId")
                .GetInt32()
                .Should()
                .Be(-1);
        }

        var correctedPayload = JsonNode.Parse(detailsPage.GetPayload())!;
        correctedPayload["data"]!["customerId"] = 123456;

        detailsPage.UpdatePayload(correctedPayload.ToJsonString());
        detailsPage.ClickReplayFixedMessage();

        var pageLoaded = messagesPage.IsLoaded();

        pageLoaded.Should().BeTrue();

        messagesPage.WaitForMessageToDisappear("E2E-CORRECT-PAYLOAD-001");

        messagesPage.GetSourceIds()
            .Should()
            .NotContain("E2E-CORRECT-PAYLOAD-001");
    }

}