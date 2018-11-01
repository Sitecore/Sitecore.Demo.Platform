using Newtonsoft.Json;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Sitecore.HabitatHome.Website.Test
{
    public class SeleniumTest
    {
        private IWebDriver driver;
        protected IWebDriver Driver
        {
            get
            {
                if (driver != null)
                    return driver;

                driver = new ChromeDriver();
                return driver;
            }
        }

        public string Host { get; set; } = "http://habitathome.dev.local";



        [SetUp]
        public void Init()
        {
            try
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                path = Path.GetDirectoryName(path);
                string json = File.ReadAllText(Path.Combine(path, GetType().Name + ".json"));
                JsonConvert.PopulateObject(json, this);
            }
            catch (Exception ex)
            {
                // Just log the exception and rely on defaults.
                // If it's catestrophic, the tests will fail.
                Console.WriteLine(ex.ToString());
            }
        }



        protected void Click(string descriptor)
        {
            Console.WriteLine($"clicking \"{descriptor}\"");
            GetElement(descriptor).Click();
        }



        protected bool Exists(string descriptor)
        {
            try
            {
                GetElement(descriptor);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }



        protected IWebElement GetElement(string descriptor)
        {
            IWebElement element = GetElements(descriptor).FirstOrDefault();
            if (element == null)
                throw new NoSuchElementException($"Could not find element \"{descriptor}\"");
            return element;
        }



        protected IEnumerable<IWebElement> GetElements(string descriptor)
        {
            var elements = Driver.FindElements(By.PartialLinkText(descriptor));
            if (!elements.Any())
                elements = Driver.FindElements(By.CssSelector(descriptor));
            if (!elements.Any())
                elements = Driver.FindElements(By.Name(descriptor));
            return elements;
        }



        protected void GoTo(string url)
        {
            Console.WriteLine($"going to {url}");
            Driver.Navigate().GoToUrl(url);
        }



        protected void Wait(int milliseconds)
        {
            Console.WriteLine($"waiting {milliseconds / 1000d} second{(milliseconds == 1000 ? "" : "s")}");
            System.Threading.Thread.Sleep(milliseconds);
        }



        protected IWebElement Wait(string descriptor, int? milliseconds = null)
        {
            Console.WriteLine($"waiting for element \"{descriptor}\"");

            var start = DateTime.Now;
            while (milliseconds == null || (DateTime.Now - start).TotalMilliseconds < milliseconds)
            {
                try
                {
                    return GetElement(descriptor);
                }
                catch
                {
                    // Wait a tiny amount of time and try again
                    System.Threading.Thread.Sleep(200);
                }
            }
            throw new TimeoutException($"Element \"{descriptor}\" never appeard within {milliseconds / 1000d} second{(milliseconds == 1000 ? "" : "s")}.");
        }

    }
}