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
    private readonly TestEnvironmentFixture _fixture;

    public FailedMessagesTests(TestEnvironmentFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void AdminHomePage_LoadsSuccessfully()
    {
        using var driver = CreateDriver();
        LoginAsAdmin(driver);
        
        var page = new FailedMessagesPage(driver);

        page.Navigate();
        page.IsLoaded().Should().BeTrue();
    }

    [Fact]
    public void FailedMessagesPage_DisplaysSeededMessages()
    {
        using var driver = CreateDriver();
        LoginAsAdmin(driver);
        var page = new FailedMessagesPage(driver);

        NavigateToFailedMessagesPage(page);

        var sourceIds = page.GetSourceIds();
        sourceIds.Should().Contain(
            "E2E-TRANSIENT-001",
            "E2E-PERMANENT-001",
            "E2E-CORRECT-PAYLOAD-001");
    }

    [Fact]
    public void FailedMessageDetails_DisplaysCorrectMessageAndPayload()
    {
        using var driver = CreateDriver();
        LoginAsAdmin(driver);
        var messagesPage = new FailedMessagesPage(driver);

        NavigateToFailedMessagesPage(messagesPage);
        messagesPage.OpenDetails("E2E-CORRECT-PAYLOAD-001");

        var detailsPage = new FailedMessageDetailsPage(driver);

        detailsPage.IsLoaded().Should().BeTrue();
        detailsPage.GetSourceId().Should().Be("E2E-CORRECT-PAYLOAD-001");
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
        using var driver = CreateDriver();
        LoginAsAdmin(driver);
        var messagesPage = new FailedMessagesPage(driver);

        NavigateToFailedMessagesPage(messagesPage);
        messagesPage.OpenDetails("E2E-CORRECT-PAYLOAD-001");

        var detailsPage = new FailedMessageDetailsPage(driver);

        detailsPage.IsLoaded().Should().BeTrue();
        detailsPage.GetSourceId().Should().Be("E2E-CORRECT-PAYLOAD-001");

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

        messagesPage.IsLoaded().Should().BeTrue();
        messagesPage.WaitForMessageToDisappear("E2E-CORRECT-PAYLOAD-001");
        messagesPage.GetSourceIds().Should().NotContain("E2E-CORRECT-PAYLOAD-001");
    }

    private RemoteWebDriver CreateDriver()
    {
        var options = new ChromeOptions();
        options.AddArgument(
        "--unsafely-treat-insecure-origin-as-secure=http://admin:8080");
        return new RemoteWebDriver(
            new Uri(E2ETestSettings.SeleniumUrl),
            options.ToCapabilities());
    }

    private void NavigateToFailedMessagesPage(FailedMessagesPage page)
    {
        page.Navigate();
        page.IsLoaded().Should().BeTrue(
            "the Failed Messages page should load before checking its data");
    }

    private void LoginAsAdmin(RemoteWebDriver driver)
    {
        var username = Environment.GetEnvironmentVariable(
            "E2E_AUTH0_USERNAME");

        var password = Environment.GetEnvironmentVariable(
            "E2E_AUTH0_PASSWORD");

        if (string.IsNullOrWhiteSpace(username)
            || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Set E2E_AUTH0_USERNAME and E2E_AUTH0_PASSWORD " +
                "environment variables before running E2E tests.");
        }

        var loginPage = new LoginPage(driver);
        loginPage.Login(username, password);
    }

}