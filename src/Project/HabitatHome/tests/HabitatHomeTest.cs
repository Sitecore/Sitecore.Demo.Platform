// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HabitatHomeTest.cs" company="">
//   
// </copyright>
// <summary>
//   The habitat home test.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

#region Information

// HabitatHome.TDS
// 
// Last Modified: 2019-07-04
// File Created: 2019-06-19

#endregion

namespace Sitecore.HabitatHome.Website.Test
{
    #region using

    using System.Configuration;
    using System.Diagnostics.Contracts;
    using System.Linq;

    using NUnit.Framework;

    using OpenQA.Selenium;

    using Sitecore.Demo.Foundation.Test;

    #endregion

    /// <summary>
    /// The habitat home test.
    /// </summary>
    [TestFixture]
    public class HabitatHomeTest : SeleniumTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HabitatHomeTest"/> class.
        /// </summary>
        public HabitatHomeTest()
        {
            var settings = ConfigurationManager.AppSettings;
            this.Host = settings["Host"];
            this.UserEmail = settings["UserEmail"];
            this.UserPassword = settings["UserPassword"];
        }

        /// <summary>
        /// Gets or sets the host.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the user email.
        /// </summary>
        public string UserEmail { get; set; }

        /// <summary>
        /// Gets or sets the user password.
        /// </summary>
        public string UserPassword { get; set; }

        /// <summary>
        /// The test account deletion.
        /// </summary>
        [Test]
        public void TestAccountDeletion()
        {
            this.Login();
            this.DeleteAccount();
            this.TakeScreenshot("01-DeleteAccountResult");
        }

        /// <summary>
        /// The test french canada visuals.
        /// </summary>
        [Test]
        public void TestFrenchCanadaVisuals()
        {
            this.GoTo(this.Host);
            this.ConfirmCookies();
            const string Language = "fr-CA";
            this.ChangeLanguage(Language);

            this.TakeScreenshot("01-TopNavigation", Language);

            this.Click("#header div.megadrop");
            this.HoverOn("Électroménagers");
            this.TakeScreenshot("02-TopNavigationOpen", Language);

            this.TakeScreenshot(
                "03-Promo-ConnectedLiving",
                $"div.field-promolink a[href*='/{Language}/home-entertainment']",
                Language);
            this.TakeScreenshot(
                "04-Promo-Gaming",
                $"div.field-promolink a[href*='/{Language}/guides/gaming']",
                Language);

            this.TakeScreenshot("05-Footer", "#footer", Language);
        }

        /// <summary>
        /// The test japanese visuals.
        /// </summary>
        [Test]
        public void TestJapaneseVisuals()
        {
            this.GoTo(this.Host);
            this.ConfirmCookies();
            const string Language = "ja-JP";
            this.ChangeLanguage(Language);

            this.TakeScreenshot("01--TopNavigation", Language);

            this.Click("#header div.megadrop");
            this.HoverOn("家電製品");
            this.TakeScreenshot("02-TopNavigationOpen", Language);

            this.TakeScreenshot(
                "03-Promo-ConnectedLiving",
                $"div.field-promolink a[href*='/{Language}/home-entertainment']",
                Language);
            this.TakeScreenshot(
                "04-Promo-Gaming",
                $"div.field-promolink a[href*='/{Language}/guides/gaming']",
                Language);

            this.TakeScreenshot("05-Footer", "#footer", Language);
        }

        /// <summary>
        /// The test new user.
        /// </summary>
        [Test]
        public void TestNewUser()
        {
            const string Language = "en";
            this.GoTo($"{this.Host}/landing-pages/bing-smart-home-design");
            this.TakeScreenshot("01-Bing");

            this.Click("How to Design Your Smart Home");
            this.ConfirmCookies();
            this.OpenInfoPanel("Referral");
            this.TakeScreenshot("02-CampaignActivated");

            this.Click("#header a");
            this.TakeScreenshot("03-PersonalizedHomepage", "SMART HOME CHECKLIST", Language);
            this.OpenInfoPanel("Onsite Behavior");
            this.TakeScreenshot("04-CampaignTriggered");

            this.Click("START HERE");
            this.Click("label.smart-home");
            this.TakeScreenshot("05-Guide", "input[value='Next']", Language);

            this.Click("input[value='Next']");
            this.Click("label.entire-house");
            this.Click("input[value='Next']");
            this.Click("label.n-a");
            this.Click("input[value='Next']");
            this.TakeScreenshot("06-RecommendedGuides", "a[href*='dryer-stack-up']", Language);

            this.Click("a[href*='dryer-stack-up']");
            this.OpenInfoPanel("Onsite Behavior");
            this.TakeScreenshot("07-ContentFinderCompleted");
            this.Click("#header a");
            this.TakeScreenshot("08-PersonalizedHomepage2", "CONTENT FINDER", Language);

            this.GoTo($"{this.Host}/en/guides/dryer-stack-up");
            this.GetElement("input[data-sc-field-name='Email']").SendKeys("test.user@sitecore.net");
            this.Click("input[value='Sign Me Up!']");
            this.OpenInfoPanel("Personal Information");
            this.TakeScreenshot("09-IdentityEstablished");
            this.OpenInfoPanel("Onsite Behavior");
            this.TakeScreenshot("10-ContentSignup");
        }

        /// <summary>
        /// The test registration.
        /// </summary>
        [Test]
        public void TestRegistration()
        {
            this.Login(false);
            this.WaitForDocumentReady();
            var errors = this.GetElements("div.field-validation-error");
            if (!errors.Any(e => e.Text.Contains("Username or password is not valid")))
            {
                this.DeleteAccount();
            }

            this.Register();
            this.TakeScreenshot("01-RegistrationResult");
            this.Click("Logout");
            this.Login();
            this.TakeScreenshot("02-LoginResult");
        }

        /// <summary>
        /// The test visuals.
        /// </summary>
        [Test]
        public void TestVisuals()
        {
            const string Language = "en";
            this.GoTo(this.Host);
            this.ConfirmCookies();
            this.TakeScreenshot("01-TopNavigation");

            this.Click("#header div.megadrop");
            this.HoverOn("Appliances");
            this.TakeScreenshot("02-TopNavigationOpen");

            this.TakeScreenshot(
                "03-Promo-ConnectedLiving",
                "div.field-promolink a[href*='home-entertainment']",
                Language);
            this.TakeScreenshot("04-Promo-Gaming", "div.field-promolink a[href*='guides/gaming']", Language);

            this.TakeScreenshot("05-Footer", "#footer");
        }

        /// <summary>
        /// The confirm cookies.
        /// </summary>
        protected void ConfirmCookies()
        {
            try
            {
                // If cookie warning of doom is present, click confirm to remove it.
                var element = this.GetElement("div.privacy-warning div.submit a");
                if (element != null)
                {
                    this.Click(element);
                }
            }
            catch (NoSuchElementException)
            {
                // Cookie doom box must not be present.  Do nothing.  All is well.
            }
        }

        /// <summary>
        /// The delete account.
        /// </summary>
        protected void DeleteAccount()
        {
            this.GoTo(this.Host);
            this.Click("MY ACCOUNT");
            this.Click("Delete Account");
            this.Click("input[type='submit']");
            this.AcceptAlert();
        }

        /// <summary>
        /// The get user.
        /// </summary>
        /// <returns>
        /// The <see cref="User"/>.
        /// </returns>
        protected User GetUser()
        {
            Contract.Ensures(Contract.Result<User>() != null);

            var user = new User { Email = this.UserEmail, Password = this.UserPassword };

            var name = (user.Email ?? string.Empty).Split('@')[0];

            var delimiter = GetUserNameDelimiter(name);

            if (delimiter == null)
            {
                user.FirstName = Capitalize(name);
                user.LastName = "User";
            }
            else
            {
                user.FirstName = Capitalize(name.Split(delimiter.Value)[0]);
                user.LastName = Capitalize(name.Split(delimiter.Value)[1]);
            }

            return user;
        }

        /// <summary>
        /// The login.
        /// </summary>
        /// <param name="doRegistrationIfMissing">
        /// The do registration if missing.
        /// </param>
        protected void Login(bool doRegistrationIfMissing = true)
        {
            var user = this.GetUser();

            this.GoTo(this.Host);
            this.ConfirmCookies();
            this.Click("LOGIN");

            this.EnterText("#loginEmail", user.Email);
            this.EnterText("#loginPassword", user.Password);
            this.Click("input[type='submit']");

            if (!this.GetElements("Logout").Any() && doRegistrationIfMissing)
            {
                this.Register();
            }
        }

        /// <summary>
        /// The open info panel.
        /// </summary>
        /// <param name="panelText">
        /// The panel text.
        /// </param>
        protected void OpenInfoPanel(string panelText)
        {
            this.Wait("button.btn-info.sidebar-closed");
            var element = this.GetElement("button.btn-info.sidebar-closed");
            if (element.Displayed)
            {
                this.Click(element);
            }

            this.Click(panelText);
            this.Wait(1000);
            var elements = this.GetElements("#sidebar div.panel-primary");
            this.ScrollTo(elements.LastOrDefault());
        }

        /// <summary>
        /// The register.
        /// </summary>
        protected void Register()
        {
            var user = this.GetUser();

            this.GoTo(this.Host);
            this.ConfirmCookies();

            this.Click("LOGIN");
            this.Click("CREATE ACCOUNT");

            this.EnterText("#registerFirstName", user.FirstName);
            this.EnterText("#registerLastName", user.LastName);
            this.EnterText("#registerEmail", user.Email);
            this.EnterText("#registerPassword", user.Password);
            this.EnterText("#registerConfirmPassword", user.Password);
            this.Click("input[type='submit']");
        }

       

        /// <summary>
        /// The change language.
        /// </summary>
        /// <param name="language">
        /// The language.
        /// </param>
        private void ChangeLanguage(string language)
        {
            this.Click("a.language-selector-select-link");
            this.Click($"a[href*='/{language}']", 1000);
        }

        /// <summary>
        /// The user.
        /// </summary>
        public class User
        {
            /// <summary>
            /// Gets or sets the email.
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            /// Gets or sets the first name.
            /// </summary>
            public string FirstName { get; set; }

            /// <summary>
            /// Gets or sets the last name.
            /// </summary>
            public string LastName { get; set; }

            /// <summary>
            /// Gets or sets the password.
            /// </summary>
            public string Password { get; set; }
        }
    }
}