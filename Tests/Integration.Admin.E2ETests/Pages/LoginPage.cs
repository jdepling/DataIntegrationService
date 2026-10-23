
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Integration.Admin.E2ETests.Configuration;

namespace Integration.Admin.E2ETests.Pages;

public sealed class LoginPage
{
    private static readonly By UsernameInput = By.Id("username");
    private static readonly By PasswordInput = By.Id("password");
    private static readonly By ContinueButton =
        By.CssSelector("button[data-action-button-primary='true']");

    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public LoginPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
    }

    public void Login(string username, string password)
    {
        _driver.Navigate().GoToUrl(
            $"{E2ETestSettings.AdminUrl.TrimEnd('/')}/Account/Login");

        _wait.Until(driver =>
            driver.FindElements(UsernameInput).Any());

        _driver.FindElement(UsernameInput).SendKeys(username);
        _driver.FindElement(ContinueButton).Click();

        _wait.Until(driver =>
            driver.FindElements(PasswordInput).Any());

        _driver.FindElement(PasswordInput).SendKeys(password);
        _driver.FindElement(ContinueButton).Click();

        _wait.Until(driver =>
            Uri.TryCreate(driver.Url, UriKind.Absolute, out var uri)
            && uri.Host.Equals(
                new Uri(E2ETestSettings.AdminUrl).Host,
                StringComparison.OrdinalIgnoreCase));
    }
}
