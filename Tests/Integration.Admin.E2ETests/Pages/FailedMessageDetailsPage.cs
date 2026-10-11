
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Integration.Admin.E2ETests.Pages;

public sealed class FailedMessageDetailsPage
{
    private static readonly By Heading = By.TagName("h1");
    private static readonly By SourceId = By.Id("source-id");
    private static readonly By PayloadTextArea = By.Name("payload");
    private static readonly By ReplayFixedButton = By.Id("replay-fixed-button");

    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public FailedMessageDetailsPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
    }

    public bool IsLoaded()
    {
        try
        {
            return _wait.Until(driver =>
                driver.FindElement(Heading).Text.Trim()
                    == "Failed Message Details");
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }

    public string GetSourceId()
    {
        return _wait.Until(driver =>
        {
            var element = driver.FindElement(SourceId);
            var value = element.Text.Trim();

            return string.IsNullOrWhiteSpace(value) ? null : value;
        })!;
    }

    public string GetPayload()
    {
        return _wait.Until(driver =>
        {
            var value = driver
                .FindElement(PayloadTextArea)
                .GetAttribute("value");

            return value;
        })!;
    }

    public bool IsReplayButtonDisplayed()
    {
        return _driver.FindElement(ReplayFixedButton).Displayed;
    }

    public void UpdatePayload(string payload)
    {
        var textArea = _wait.Until(driver =>
        {
            var element = driver.FindElement(PayloadTextArea);
            return element.Displayed && element.Enabled ? element : null;
        });

        textArea.Clear();
        textArea.SendKeys(payload);
    }

    public void ClickReplayFixedMessage()
    {
        var button = _wait.Until(driver =>
        {
            var element = driver.FindElement(ReplayFixedButton);

            return element.Displayed && element.Enabled
                ? element
                : null;
        });

        ((IJavaScriptExecutor)_driver).ExecuteScript(
            "arguments[0].scrollIntoView({ block: 'center' });",
            button);

        _wait.Until(_ =>
        {
            try
            {
                button.Click();
                return true;
            }
            catch (ElementClickInterceptedException)
            {
                return false;
            }
        });
    }
}
