using System.ComponentModel.DataAnnotations;
using Sitecore.HabitatHome.Feature.Accounts.Attributes;
using Sitecore.HabitatHome.Foundation.Dictionary.Repositories;

namespace Sitecore.HabitatHome.Feature.Accounts.Models
{                                                             
    public class LoginInfo
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

        public static string EmailCaption => DictionaryPhraseRepository.Current.Get("/Accounts/Login/Email", "E-mail");

        public static string PasswordCaption => DictionaryPhraseRepository.Current.Get("/Accounts/Login/Password", "Password");

        public static string MinimumPasswordLength => DictionaryPhraseRepository.Current.Get("/Accounts/Login/Minimum Password Length", "Please enter a password with at lease {1} characters");

        public static string Required => DictionaryPhraseRepository.Current.Get("/Accounts/Login/Required", "Please enter a value for {0}");

        public static string InvalidEmailAddress => DictionaryPhraseRepository.Current.Get("/Accounts/Login/Invalid Email Address", "Please enter a valid email address");
    }
}