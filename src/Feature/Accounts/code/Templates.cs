namespace Sitecore.HabitatHome.Feature.Accounts
{
    using Sitecore.Data;

    public struct Templates
    {                
        public struct AccountsSettings
        {
            public static readonly ID ID = new ID("{4C7D90D2-CEAA-4A1F-8302-63D831E8E7B6}");

            public struct Fields
            {
                public static readonly ID AccountsDetailsPage = new ID("{A98F8B72-64AA-4A9C-90DB-92F7AEAD55AD}");
                public static readonly ID RegisterPage = new ID("{81B370B0-9EA6-435D-BE15-FCFFB1CEC57F}");
                public static readonly ID LoginPage = new ID("{E90C6342-5BC9-4011-A3C5-EED1FCA7EB7C}");
                public static readonly ID ForgotPasswordPage = new ID("{927F708E-AB61-44AB-AB09-2A1A23E07E05}");
                public static readonly ID AfterLoginPage = new ID("{B8937BFD-64C0-4ABE-8E64-B328EA3F1EA1}");
                public static readonly ID ForgotPasswordMailTemplate = new ID("{F2F9CC39-FDA5-4A19-82DA-D03699D8F1BE}");
                public static readonly ID RegisterOutcome = new ID("{F48206DF-D2EB-4DF2-9AC0-18A2E9C31F29}");
            }
        }

        public struct UserProfile
        {
            public static readonly ID ID = new ID("{BCBF635A-9C02-4EFE-85FB-19358FAEAAB2}");

            public struct Fields
            {
                public static readonly ID FirstName = new ID("{A503A07D-DA64-4578-AC8C-864394DBB00E}");
                public static readonly ID LastName = new ID("{0A826D28-B06F-45DD-BAD7-5ED5677A9F2D}");
                public static readonly ID PhoneNumber = new ID("{B9B7898C-44EC-4D47-89E9-D868866E5618}");
                public static readonly ID Interest = new ID("{F53668CA-0B33-45A4-83CF-A6B09687BC64}");
            }
        }

        public struct ProfileSettigs
        {
            public static readonly ID ID = new ID("{7E5DE17D-5975-40B7-952E-45E646699318}");

            public struct Fields
            {
                public static readonly ID UserProfile = new ID("{202EAAE4-33B3-4DB1-9170-4DE88A27E7CD}");
                public static readonly ID InterestsFolder = new ID("{7DEB567C-87FA-48CE-9ADC-8F35E05DDE91}");
            }
        }

        public struct Interest
        {
            public static readonly ID ID = new ID("{CB2C27F8-01F4-48FF-82F5-5256F8B1FDC3}");

            public struct Fields
            {
                public static readonly ID Title = new ID("{CE693119-C97F-48FD-8154-27C4930CA7DB}");
            }
        }

        public struct MailTemplate
        {
            public static readonly ID ID = new ID("{2E4EA6E2-73FA-423B-9338-730C380B84E2}");

            public struct Fields
            {
                public static readonly ID From = new ID("{D8A24409-0AEC-4C55-9513-5028E50D6AAB}");
                public static readonly ID Subject = new ID("{FAF6C7D4-1C02-4986-9D8E-EDF726BD92B8}");
                public static readonly ID Body = new ID("{BCB0298C-F7FA-437F-BC29-81A8C2D0BD6D}");
            }
        }
    }
}