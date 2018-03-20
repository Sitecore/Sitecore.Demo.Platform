using Sitecore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Feature.Email
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
    }
}