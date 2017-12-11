namespace Sitecore.Feature.Accounts.Models
{
    using System.ComponentModel.DataAnnotations;

    public class PasswordResetInfo
    {
        [Display(Name = nameof(EmailCaption), ResourceType = typeof(PasswordResetInfo))]
        [Required(ErrorMessageResourceName = nameof(Required), ErrorMessageResourceType = typeof(PasswordResetInfo))]
        [EmailAddress(ErrorMessageResourceName = nameof(InvalidEmailAddress), ErrorMessageResourceType = typeof(PasswordResetInfo))]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public static string EmailCaption => Sitecore.Globalization.Translate.Text("Email");
        public static string Required => Sitecore.Globalization.Translate.Text("FieldRequired");
        public static string InvalidEmailAddress => Sitecore.Globalization.Translate.Text("InvalidEmail");
    }
}