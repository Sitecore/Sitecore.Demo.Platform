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
            Click("How to Design Your Smart Home");
            Wait("button.btn-info.sidebar-closed");
            Click("button.btn-info.sidebar-closed");
            Click("Referral");

            var elements = GetElements("#referralPanel div.media");
            Assert.IsTrue(!elements.Any(e => e.Text.Contains("You have not been associated with any campaigns")));
        }

    }
}