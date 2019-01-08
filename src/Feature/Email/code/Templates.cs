using Sitecore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.HabitatHome.Feature.Email
{
    public struct Templates
    {
        public struct Header
        {
            public static readonly ID ID = new ID("{8B66B32F-1C8B-40E6-8008-99A4B5361858}");
            public struct Fields
            {
                public static readonly ID Logo = new ID("{D839F656-CA1E-404D-8250-2750057712E0}");
            }
        }    

        public struct Footer
        {
            public static readonly ID ID = new ID("{D7776780-D054-4C97-A12A-EF41C70CC8E2}");

            public struct Fields
            {
                public static readonly ID Logo = new ID("{C32E54A4-E8A6-4968-B7B2-E42351043D27}");
                public static readonly ID Text = new ID("{C699C120-B1A1-4884-8951-F476C8C95AB3}");
                public static readonly ID Copyright = new ID("{61ABBFBA-383B-4E6C-B744-E714B4CC1A4F}");
                public static readonly ID FacebookLogo = new ID("{F2205047-2736-412F-AA5C-0CE074859943}");
                public static readonly ID FacebookLink = new ID("{E36931DE-C4B5-4DBD-B54B-AA67BBE50F7A}");
                public static readonly ID TwitterLogo = new ID("{B620D0E4-40D6-4292-8DD7-366473CBF721}");
                public static readonly ID TwitterLink = new ID("{F62307DE-AC61-4808-9747-092AB8FBA8B9}");
                public static readonly ID YoutubeLogo = new ID("{A0775839-F666-4348-B080-F2618F4D999C}");
                public static readonly ID YoutubeLink = new ID("{4EEE16D0-D4E4-4894-A9CB-AF0B47B6F969}");
                public static readonly ID LinkedinLogo = new ID("{EDA7563A-AC3F-4A89-9AD9-1DFAFE802640}");
                public static readonly ID LinkedinLink = new ID("{425F54E4-BAE3-4C2A-BC5C-9EFEAA74926E}");

            }
        }

        public struct EmailBody
        {
            public static readonly ID ID = new ID("{ED00543C-2517-48AD-A761-3476FE3B4FB6}");

            public struct Fields
            {
                public static readonly ID Title = new ID("{9C4FE3FF-D76E-462B-97B7-80DBB35A80DF}");
                public static readonly ID Body = new ID("{9F2D5F68-061F-43A5-AA1C-12079D07763E}");
            }
        }

        public struct Promo
        {
            public struct Fields
            {       
                public static readonly ID Title = new ID("{2BE2FD2C-B43F-4237-9B24-2905EFE66483}");
                public static readonly ID PromoText = new ID("{EFFCB8E2-8C7E-4CA8-87EA-6E9B9D120C13}");
                public static readonly ID LinkLabel = new ID("{330DC4A3-644B-4ED4-8775-F7313DCAB71F}");
                public static readonly ID Image = new ID("{270CC513-EE8A-4D95-87A3-9EF5129733AD}");
                public static readonly ID Link = new ID("{4521AD3A-5672-409A-86C1-58362E88DDB6}");
            }
        }

        public struct PageTeaser
        {
            public struct Fields
            {
                public static readonly ID Title = new ID("{E2A634FD-5177-46DE-B629-5393CAED77D7}");
                public static readonly ID Summary = new ID("{CA65ACA5-4323-418A-AA72-A5037F606D2A}");
                public static readonly ID Content = new ID("{5A0B08E5-7F6C-48DC-929A-5D6CFDA45E1D}");
                public static readonly ID Image = new ID("{23AA5741-FA6A-4ADD-9E79-7A752AF6DEB1}");
                public static readonly ID OpenGraphImage = new ID("{D8B38DD7-E9A0-4410-9F17-D436B0C8635A}");
            }
        }

        public struct Image
        {
            public struct Fields
            {
                public static readonly ID Image = new ID("{EBE4FDEF-16D5-4D19-833C-24A241628A3A}");
            }
        }

    }
}