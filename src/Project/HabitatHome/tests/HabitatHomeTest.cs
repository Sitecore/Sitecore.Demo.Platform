using NUnit.Framework;
using System.Linq;

namespace Sitecore.HabitatHome.Website.Test
{
    [TestFixture]
    public class HabitatHomeTest : SeleniumTest
    {
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

    }
}