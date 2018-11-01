using NUnit.Framework;
using System;
using System.Linq;

namespace Sitecore.HabitatHome.Website.Test
{
    [TestFixture]
    public class HabitatHomeTest : SeleniumTest
    {
        public class User
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }



        protected User CreateUser()
        {
            var user = new User();
            user.FirstName = DateTime.Now.ToString("MMMM");
            user.LastName = NumberToWords(DateTime.Now.Day);
            user.LastName = user.LastName.Replace(" ", "");
            user.LastName = user.LastName.Substring(0, 1).ToUpper() +
                user.LastName.Substring(1);
            user.Email = $"{user.FirstName.ToLower()}.{user.LastName.ToLower()}{DateTime.Now.ToString("hhmm")}@sitecore.net";
            user.Password = "habitat";
            return user;
        }



        protected void LoginUser(User user)
        {
            Click("LOGIN");
            EnterText("#loginEmail", user.Email);
            EnterText("#loginPassword", user.Password);
            Click("input[type='submit']");
        }



        protected static string NumberToWords(int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += " " + unitsMap[number % 10];
                }
            }

            return words;
        }



        protected void RegisterUser(User user)
        {
            GoTo($"{Host}");
            Click("LOGIN");
            Click("CREATE ACCOUNT");

            EnterText("#registerFirstName", user.FirstName);
            EnterText("#registerLastName", user.LastName);
            EnterText("#registerEmail", user.Email);
            EnterText("#registerPassword", user.Password);
            EnterText("#registerConfirmPassword", user.Password);
            Click("input[type='submit']");
        }



        [Test]
        public void TestNewUser()
        {
            GoTo($"{Host}/landing-pages/bing-smart-home-design");
            TakeScreenshot("01-Bing");

            Click("How to Design Your Smart Home");
            // If cookie warning of doom is present, click confirm to remove it.
            var element = GetElement("div.privacy-warning div.submit a");
            if (element != null)
                Click(element);
            Wait("button.btn-info.sidebar-closed");
            Click("button.btn-info.sidebar-closed");
            Click("Referral");
            TakeScreenshot("02-CampaignActivated");

            Click("a[title='Logo']");
            TakeScreenshot("03-PersonalizedHomepage", "SMART HOME CHECKLIST");
            Wait("button.btn-info.sidebar-closed");
            Click("button.btn-info.sidebar-closed");
            Click("Onsite Behavior");
            var elements = GetElements("#onSiteBehaviorProfileCurrent div.progress-bar.progress-bar-success");
            TakeScreenshot("04-CampaignTriggered", elements.LastOrDefault());

            Click("START HERE");
            Click("label.smart-home");
            TakeScreenshot("05-Guide", "input[value='Next']");

            Click("input[value='Next']");
            Click("label.entire-house");
            Click("input[value='Next']");
            Click("label.n-a");
            Click("input[value='Next']");
            TakeScreenshot("06-RecommendedGuides", "a[href*='dryer-stack-up']");

            Click("a[href*='dryer-stack-up']");
            Wait("button.btn-info.sidebar-closed");
            Click("button.btn-info.sidebar-closed");
            Click("Onsite Behavior");
            elements = GetElements("#onsiteBehaviorPanel div.media div.text-nowrap");
            TakeScreenshot("07-ContentFinderCompleted", elements.FirstOrDefault(e => e.Text.Contains("Content Finder Completed")));
            Click("a[title='Logo']");
            TakeScreenshot("08-PersonalizedHomepage2", "CONTENT FINDER");

            GoTo($"{Host}/en/guides/dryer-stack-up");
            GetElement("input[data-sc-field-name='Email']").SendKeys("test.user@sitecore.net");
            Click("input[value='Sign Me Up!']");
            Wait("button.btn-info.sidebar-closed");
            Click("button.btn-info.sidebar-closed");
            Click("Personal Information");
            TakeScreenshot("09-IdentityEstablished");
            Click("Onsite Behavior");
            elements = GetElements("#onsiteBehaviorPanel div.media div.text-nowrap");
            TakeScreenshot("10-ContentSignup", elements.FirstOrDefault(e => e.Text.Contains("Content Sign up")));
        }



        [Test]
        public void TestRegistration()
        {
            var user = CreateUser();
            RegisterUser(user);
            TakeScreenshot("01-RegistrationResult");
            Click("Logout");
            LoginUser(user);
            TakeScreenshot("02-LoginResult");
        }

    }
}