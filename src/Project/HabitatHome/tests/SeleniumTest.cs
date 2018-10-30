using Newtonsoft.Json;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Sitecore.HabitatHome.Website.Test
{
    public class SeleniumTest
    {
        private string bitmapsPath;
        private static string codeBasePath;
        private IWebDriver driver;



        protected void ApplyConfig()
        {
            try
            {
                string jsonFilename = Path.Combine(CodeBasePath, GetType().Name + ".json");
                string json = File.ReadAllText(jsonFilename);
                JsonConvert.PopulateObject(json, this);
            }
            catch (Exception ex)
            {
                // Just log the exception and rely on defaults.
                // If it's catestrophic, the tests will fail.
                Console.WriteLine(ex.ToString());
            }
        }



        public string BackstopPath { get; set; } = FindBackstopPath();



        public string BitmapsPath
        {
            get
            {
                return bitmapsPath ??
                    (bitmapsPath = Path.Combine(BackstopPath, "backstop_data", "bitmaps_test",
                        DateTime.Now.ToString("yyyyMMdd-HHmmss")));
            }
        }



        protected static string CodeBasePath
        {
            get
            {
                if (codeBasePath != null)
                    return codeBasePath;

                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                codeBasePath = Path.GetDirectoryName(path);
                return CodeBasePath;
            }
        }



        protected void Click(string descriptor)
        {
            Console.WriteLine($"clicking \"{descriptor}\"");
            GetElement(descriptor).Click();
        }



        protected IWebDriver Driver
        {
            get
            {
                if (driver != null)
                    return driver;

                driver = new ChromeDriver();
                driver.Manage().Window.Size = new Size(1200, 800);
                return driver;
            }
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



        protected static string FindBackstopPath()
        {
            string path = CodeBasePath;
            string backstopPath = Path.Combine(path, "backstop");
            while (!Directory.Exists(Path.Combine(backstopPath, "backstop_data")))
            {
                DirectoryInfo dir = Directory.GetParent(path);

                // If dir is null, then we've climbed the directory tree
                // as far as we can go.  The backstop directory either
                // doesn't exist or it is somewhere out of reach.  Just
                // return the CodeBasePath.
                if (dir == null)
                    return CodeBasePath;

                path = dir.FullName;
                backstopPath = Path.Combine(path, "backstop");
            }
            return backstopPath;
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



        public string Host { get; set; } = "http://habitathome.dev.local";



        public void TakeScreenshot(string name)
        {
            var its = driver as ITakesScreenshot;
            Screenshot s = its.GetScreenshot();
            Directory.CreateDirectory(BitmapsPath);
            name = Path.Combine(BitmapsPath,
                $"{GetType().Name}_{TestContext.CurrentContext.Test.Name}_{name}.png");
            s.SaveAsFile(name, ScreenshotImageFormat.Png);
        }



        [TearDown]
        public void TestCleanup()
        {
            if (driver != null)
            {
                TakeScreenshot("final");
                driver.Dispose();
            }
        }



        [SetUp]
        public void TestInit()
        {
            ApplyConfig();
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