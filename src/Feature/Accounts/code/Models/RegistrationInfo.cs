namespace Sitecore.Feature.Accounts.Models
{
    using Sitecore.Feature.Accounts.Attributes;
    using Sitecore.XA.Foundation.Mvc.Models;
    using System.ComponentModel.DataAnnotations;

    public class RegistrationInfo : RenderingModelBase
    {
        [Display(Name = nameof(EmailCaption), ResourceType = typeof(RegistrationInfo))]
        [Required(ErrorMessageResourceName = nameof(Required), ErrorMessageResourceType = typeof(RegistrationInfo))]
        [EmailAddress(ErrorMessageResourceName = nameof(InvalidEmailAddress), ErrorMessageResourceType = typeof(RegistrationInfo))]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = nameof(PasswordCaption), ResourceType = typeof(RegistrationInfo))]
        [Required(ErrorMessageResourceName = nameof(Required), ErrorMessageResourceType = typeof(RegistrationInfo))]
        [PasswordMinLength(ErrorMessageResourceName = nameof(MinimumPasswordLength), ErrorMessageResourceType = typeof(RegistrationInfo))]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = nameof(ConfirmPasswordCaption), ResourceType = typeof(RegistrationInfo))]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessageResourceName = nameof(ConfirmPasswordMismatch), ErrorMessageResourceType = typeof(RegistrationInfo))]
        public string ConfirmPassword { get; set; }

        public string ReturnUrl { get; set; }

        public static string ConfirmPasswordCaption => Sitecore.Globalization.Translate.Text("ConfirmPassword");
        public static string EmailCaption => Sitecore.Globalization.Translate.Text("Email");
        public static string PasswordCaption => Sitecore.Globalization.Translate.Text("Password");
        public static string ConfirmPasswordMismatch => Sitecore.Globalization.Translate.Text("PasswordMismatch");
        public static string MinimumPasswordLength => Sitecore.Globalization.Translate.Text("MinPasswordLength");
        public static string Required => Sitecore.Globalization.Translate.Text("EnterValueFor");
        public static string InvalidEmailAddress => Sitecore.Globalization.Translate.Text("InvalidEmail");
    }
}