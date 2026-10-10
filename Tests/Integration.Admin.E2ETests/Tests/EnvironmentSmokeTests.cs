
using Integration.Admin.E2ETests.Fixtures;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;

namespace Integration.Admin.E2ETests.Tests;

public sealed class EnvironmentSmokeTests : IClassFixture<TestEnvironmentFixture>
{
    public EnvironmentSmokeTests(TestEnvironmentFixture fixture)
    {
    }

    [Fact]
    public void AdminHomePage_LoadsSuccessfully()
    {
        var options = new ChromeOptions();

        using var driver = new RemoteWebDriver(
            new Uri("http://localhost:4445"),
            options.ToCapabilities());

        driver.Navigate().GoToUrl("http://admin:8080/");

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

        var body = wait.Until(d =>
        {
            var element = d.FindElement(By.TagName("body"));

            return element.Text.Contains(
                "Failed Messages",
                StringComparison.OrdinalIgnoreCase)
                ? element
                : null;
        });

        Assert.Contains(
            "Failed Messages",
            body!.Text,
            StringComparison.OrdinalIgnoreCase);
    }
}
