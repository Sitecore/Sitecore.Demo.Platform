using System;
using Sitecore.XConnect;

namespace Sitecore.Demo.Platform.Foundation.Accounts.Models.Facets
{
    [Serializable]
    public class SportName : Facet
    {
        public static string DefaultKey = "SportName";

        public string Value { get; set; }
    }
}