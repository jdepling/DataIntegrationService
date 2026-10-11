using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Integration.Admin.E2ETests.Configuration;

namespace Integration.Admin.E2ETests.Pages;

public sealed class FailedMessagesPage
{
    private static readonly By Body = By.TagName("body");
    private static readonly By MessageRows = By.CssSelector("table tbody tr");
    private static readonly By SourceIdCell = By.CssSelector("td:first-child");
    private static readonly By DetailsLink = By.LinkText("Details");

    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public FailedMessagesPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
    }

    public void Navigate()
    {
        _driver.Navigate().GoToUrl(E2ETestSettings.AdminUrl);
    }

    public bool IsLoaded()
    {
        try
        {
            var body = _wait.Until(driver =>
            {
                var element = driver.FindElement(Body);

                return element.Text.Contains(
                    "Failed Messages",
                    StringComparison.OrdinalIgnoreCase)
                    ? element
                    : null;
            });

            return body is not null;
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }

    public IReadOnlyList<string> GetSourceIds()
    {
        return _driver
            .FindElements(MessageRows)
            .Select(row => row.FindElement(SourceIdCell).Text.Trim())
            .ToList();
    }

    public void OpenDetails(string sourceId)
    {
        var row = _wait.Until(driver =>
            driver.FindElements(MessageRows)
                .FirstOrDefault(row =>
                    row.FindElement(SourceIdCell).Text.Trim() == sourceId));

        if (row is null)
        {
            throw new NoSuchElementException(
                $"No failed message was found with source ID '{sourceId}'.");
        }

        row.FindElement(DetailsLink).Click();
    }

    public void WaitForMessageToDisappear(string sourceId)
    {
        _wait.Until(driver =>
            !driver.FindElements(MessageRows)
                .Any(row =>
                    row.FindElement(SourceIdCell).Text.Trim() == sourceId));
    }
}