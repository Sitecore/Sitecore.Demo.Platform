namespace Sitecore.Feature.Accounts.Models
{
    using Sitecore.Feature.Accounts.Attributes;
    using Sitecore.XA.Foundation.Mvc.Models;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class LoginInfo : RenderingModelBase
    {
        [Display(Name = nameof(EmailCaption), ResourceType = typeof(LoginInfo))]
        [Required(ErrorMessageResourceName = nameof(Required), ErrorMessageResourceType = typeof(LoginInfo))]
        [EmailAddress(ErrorMessageResourceName = nameof(InvalidEmailAddress), ErrorMessageResourceType = typeof(LoginInfo))]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = nameof(PasswordCaption), ResourceType = typeof(LoginInfo))]
        [Required(ErrorMessageResourceName = nameof(Required), ErrorMessageResourceType = typeof(LoginInfo))]
        [PasswordMinLength(ErrorMessageResourceName = nameof(MinimumPasswordLength), ErrorMessageResourceType = typeof(LoginInfo))]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
        public IEnumerable<FedAuthLoginButton> LoginButtons { get; set; }

        public static string EmailCaption => Sitecore.Globalization.Translate.Text("Email");
        public static string PasswordCaption => Sitecore.Globalization.Translate.Text("Password");
        public static string MinimumPasswordLength => Sitecore.Globalization.Translate.Text("MinPasswordLength");
        public static string Required => Sitecore.Globalization.Translate.Text("FieldIsRequired");
        public static string InvalidEmailAddress => Sitecore.Globalization.Translate.Text("InvalidEmail");
    }
}