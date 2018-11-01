using Newtonsoft.Json;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
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



        protected void Click(IWebElement element)
        {
            try
            {
                element.Click();
            }
            catch (WebDriverException wde)
            {
                if (wde.Message.Contains("is not clickable at point") &&
                    wde.Message.Contains("Other element would receive the click"))
                {
                    var js = Driver as IJavaScriptExecutor;
                    js.ExecuteScript("arguments[0].click();", element);
                }
                else
                    throw;
            }
            WaitForDocumentReady();
        }



        protected void Click(string descriptor)
        {
            Console.WriteLine($"clicking \"{descriptor}\"");
            IWebElement element = GetElement(descriptor);
            Click(element);
        }



        protected IWebDriver Driver
        {
            get
            {
                if (driver != null)
                    return driver;

                driver = new ChromeDriver();
                driver.Manage().Window.Size = new Size(1800, 1000);
                return driver;
            }
        }



        protected void EnterText(IWebElement element, string text)
        {
            element.SendKeys(text);
        }



        protected void EnterText(string descriptor, string text)
        {
            EnterText(GetElement(descriptor), text);
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
            WaitForDocumentReady();
        }



        public string Host { get; set; } = "http://habitathome.dev.local";



        public void TakeFullPageScreenshot(string name)
        {
            // Add html2canvas if it isn't already loaded.
            WaitForDocumentReady();
            var js = (Driver as IJavaScriptExecutor);
            if ((bool)js.ExecuteScript("return (typeof html2canvas == 'undefined')"))
                js.ExecuteScript("var script = document.createElement('script'); script.src = \"https://cdnjs.cloudflare.com/ajax/libs/html2canvas/0.4.1/html2canvas.min.js\"; document.body.appendChild(script);");

            // Wait for it to load
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(60));
            wait.IgnoreExceptionTypes(typeof(InvalidOperationException));
            wait.Until(wd => (bool)js.ExecuteScript("return (typeof html2canvas != 'undefined')"));

            // Take a full page screenshot using html2canvas
            string generateScreenshotJS =
                @"(function() {
	                html2canvas(document.body, {
 		                onrendered: function (canvas) {                                          
		                    window.canvasImgContentDecoded = canvas.toDataURL(""image/png"");
	                    }
                    });
                })();";
            js.ExecuteScript(generateScreenshotJS);

            // Wait for the screenshot to complete
            wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(60));
            wait.IgnoreExceptionTypes(typeof(InvalidOperationException));
            wait.Until(wd => !string.IsNullOrEmpty(js.ExecuteScript("return canvasImgContentDecoded;") as string));

            // Convert to raw bytes
            var pngContent = (string)js.ExecuteScript("return canvasImgContentDecoded;");
            pngContent = pngContent.Replace("data:image/png;base64,", string.Empty);
            byte[] data = Convert.FromBase64String(pngContent);

            // Save screenshot to BitmapsPath under backstop
            name = Path.Combine(BitmapsPath,
                $"{GetType().Name}_{TestContext.CurrentContext.Test.Name}_{name}.png");
            Directory.CreateDirectory(BitmapsPath);
            File.WriteAllBytes(name, data);
        }



        public void TakeScreenshot(string name)
        {
            TakeScreenshot(name, (IWebElement)null);
        }



        public void TakeScreenshot(string name, string onElement)
        {
            TakeScreenshot(name, GetElement(onElement));
        }



        public void TakeScreenshot(string name, IWebElement onElement)
        {
            if (onElement != null)
            {
                Actions actions = new Actions(Driver);
                actions.MoveToElement(onElement);
                actions.Perform();
            }

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
                //TakeScreenshot("final");
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



        protected void WaitForDocumentReady()
        {
            var timeout = new TimeSpan(0, 0, 60);
            var wait = new WebDriverWait(Driver, timeout);

            var javascript = Driver as IJavaScriptExecutor;
            if (javascript == null)
                throw new ArgumentException("driver", "Driver must support javascript execution");

            wait.Until((d) =>
            {
                try
                {
                    string readyState = javascript.ExecuteScript("if (document.readyState) return document.readyState;").ToString();
                    return readyState.ToLower() == "complete";
                }
                catch (Exception)
                {
                    return false;
                }
            });

        }

    }
}