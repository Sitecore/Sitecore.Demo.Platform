namespace Sitecore.Feature.Accounts.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class EditProfile
    {
        [Display(Name = nameof(EmailCaption), ResourceType = typeof(EditProfile))]
        public string Email { get; set; }

        [Display(Name = nameof(FirstNameCaption), ResourceType = typeof(EditProfile))]
        public string FirstName { get; set; }

        [Display(Name = nameof(LastNameCaption), ResourceType = typeof(EditProfile))]
        public string LastName { get; set; }

        [Display(Name = nameof(PhoneNumberCaption), ResourceType = typeof(EditProfile))]
        [RegularExpression(@"^\+?\d*(\(\d+\)-?)?\d+(-?\d+)+$", ErrorMessageResourceName = nameof(PhoneNumberFormat), ErrorMessageResourceType = typeof(EditProfile))]
        [MaxLength(20, ErrorMessageResourceName = nameof(MaxLengthExceeded), ErrorMessageResourceType = typeof(EditProfile))]
        public string PhoneNumber { get; set; }

        [Display(Name = nameof(InterestsCaption), ResourceType = typeof(EditProfile))]
        public string Interest { get; set; }

        public IEnumerable<string> InterestTypes { get; set; }

        public static string EmailCaption => Sitecore.Globalization.Translate.Text("Email");
        public static string FirstNameCaption => Sitecore.Globalization.Translate.Text("FirstName");
        public static string LastNameCaption => Sitecore.Globalization.Translate.Text("LastName");
        public static string PhoneNumberCaption => Sitecore.Globalization.Translate.Text("PhoneNumber");
        public static string InterestsCaption => Sitecore.Globalization.Translate.Text("Interests");
        public static string MaxLengthExceeded => Sitecore.Globalization.Translate.Text("MaxLength");
        public static string PhoneNumberFormat => Sitecore.Globalization.Translate.Text("PhoneNumberFormat");
    }
}