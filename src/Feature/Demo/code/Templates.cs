namespace Sitecore.HabitatHome.Feature.Demo
{
    using Sitecore.Data;

    public struct Templates
    {
        public struct ProfilingSettings
        {
            public static readonly ID ID = new ID("{C6D4DDD5-B912-4C1A-A3A3-E1D90E4D0939}");

            public struct Fields
            {
                public static readonly ID SiteProfiles = new ID("{2A84ECA4-68BB-4451-B4AC-98EA71A5A3DC}");
            }
        }

        public struct DemoContent
        {
            public static readonly ID ID = new ID("{90224634-CAB5-4D8E-9F71-2DBAC989C6F8}");

            public struct Fields
            {
                public static readonly ID HtmlContent = new ID("{621CFA47-D63B-4B8E-81E7-0CA5C2619909}");
                public static readonly ID Referrer = new ID("{1D1F11DC-B6F5-4DB3-B26A-7AE5AAC5C07B}");
                public static readonly ID IpAddress = new ID("{E0183A20-5B79-4A3E-971E-5CF8118095F1}");

                public static readonly ID Latitude = new ID("{F7B5DF32-D5AB-4911-82E0-1AD8E1E279BD}");
                public static readonly ID Longitude = new ID("{268EF40B-81F5-4842-8ADF-F2C57606C067}");
                public static readonly ID AreaCode = new ID("{D880D0AF-87E9-42C9-8B48-EB1482B4A06E}");
                public static readonly ID BusinessName = new ID("{00635EB5-6BC8-46A7-94EF-216DFC03FBB8}");
                public static readonly ID City = new ID("{B39A3F51-B73B-49FF-AB72-10FE43D486DB}");
                public static readonly ID Country = new ID("{EB60B94D-1EA5-4EBC-94D4-534C82E80F1A}");
                public static readonly ID DNS = new ID("{7224C880-CCC6-40F7-B850-E19BF1DE6D01}");
                public static readonly ID ISP = new ID("{E03F86EE-69F5-4704-8E2F-EE3EF6845589}");
                public static readonly ID MetroCode = new ID("{989E88A6-FF45-4F60-B8CD-BE07F9D92B3F}");
                public static readonly ID PostalCode = new ID("{7325190F-CE21-43B7-BA4E-FBEB1842350C}");
                public static readonly ID Region = new ID("{D0C9D21B-7B64-4E4A-905F-8697A50B18E2}");
                public static readonly ID Url = new ID("{594BAF13-3763-4C7D-86B1-B9BB46A86359}");
            }
        }           

        public struct Token
        {
            public static readonly ID ID = new ID("{4E24E58B-2CD5-499D-9BC7-2F8645510BB3}");

            public struct Fields
            {
                public static readonly string TokenValue = "Token Value";
            }
        }
    }
}