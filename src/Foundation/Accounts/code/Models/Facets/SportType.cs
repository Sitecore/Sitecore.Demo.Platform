using System;
using Sitecore.XConnect;

namespace Sitecore.Demo.Platform.Foundation.Accounts.Models.Facets
{
    [Serializable]
    public class SportType : Facet
    {
        public static string DefaultKey = "SportType";

        public string Value { get; set; }
    }
}