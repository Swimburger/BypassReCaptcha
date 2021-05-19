using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace BypassReCaptcha.UiTests
{
    [TestClass]
    public class ContactUsFormTests
    {
        private static IConfiguration configuration;

        [ClassInitialize]
        public static async Task SetupTests(TestContext testContext)
        {
            Dictionary<string, string> testParametersDictionary = new Dictionary<string, string>();
            foreach (var key in testContext.Properties.Keys)
            {
                testParametersDictionary.Add(key.ToString(), testContext.Properties[key].ToString());
            }

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets<ContactUsFormTests>(optional: true)
			    .AddEnvironmentVariables()
                .AddInMemoryCollection(testParametersDictionary);
            configuration = builder.Build();

            var chromeDriverInstaller = new ChromeDriverInstaller();
            await chromeDriverInstaller.Install();
        }

        [TestMethod]
        public void Submitting_Form_Without_ReCaptcha_Should_Throw_Exception()
        {
            Assert.ThrowsException<AssertFailedException>(() =>
            {
                var chromeOptions = new ChromeOptions();
                chromeOptions.AddArguments("headless");
                using (var driver = new ChromeDriver(chromeOptions))
                {
                    SubmitForm(driver);
                }
            });
        }

        [TestMethod]
        public void Submitting_Form_With_BypassReCaptcha_Should_Succeed()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");
            using (var driver = new ChromeDriver(chromeOptions))
            {
                BypassRecaptcha(driver);
                SubmitForm(driver);
            }
        }

        private void SubmitForm(IWebDriver driver)
        {
            driver.Navigate().GoToUrl("https://localhost:5001");
            driver.FindElement(By.Id("firstName")).SendKeys("Jon");
            driver.FindElement(By.Id("lastName")).SendKeys("Doe");
            driver.FindElement(By.Id("emailAddress")).SendKeys("jon.doe@contoso.net");
            driver.FindElement(By.Id("question")).SendKeys("Hello World!");

            driver.FindElements(By.CssSelector("form button")).First().Click();

            Assert.AreEqual("https://localhost:5001/ThankYou", driver.Url);
            Assert.IsTrue(driver.PageSource.Contains("Thank you for contacting us"));
        }

private void BypassRecaptcha(IWebDriver driver)
{
    driver.Navigate().GoToUrl("https://localhost:5001/BypassRecaptcha");
    driver.FindElement(By.Id("secret")).SendKeys(configuration.GetValue<string>("BypassSecret"));

    driver.FindElements(By.CssSelector("form button")).First().Click();

    Assert.IsTrue(driver.PageSource.Contains("Success!"));
}
    }
}