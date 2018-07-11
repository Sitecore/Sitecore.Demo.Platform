using System.ComponentModel.DataAnnotations;
using Sitecore.HabitatHome.Foundation.Dictionary.Repositories;

namespace Sitecore.HabitatHome.Feature.Accounts.Models
{
    public class ChangePasswordInfo
    {
        [Display(Name = nameof(PasswordCaption), ResourceType = typeof(ChangePasswordInfo))]
        [Required(ErrorMessageResourceName = nameof(Required), ErrorMessageResourceType = typeof(ChangePasswordInfo))]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = nameof(NewPasswordCaption), ResourceType = typeof(ChangePasswordInfo))]
        [Required(ErrorMessageResourceName = nameof(Required), ErrorMessageResourceType = typeof(ChangePasswordInfo))]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Display(Name = nameof(ConfirmPasswordCaption), ResourceType = typeof(RegistrationInfo))]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessageResourceName = nameof(ConfirmPasswordMismatch), ErrorMessageResourceType = typeof(RegistrationInfo))]
        public string ConfirmPassword { get; set; }

        public static string Required => DictionaryPhraseRepository.Current.Get("/Accounts/Forgot Password/Required", "Please enter a value for {0}");
        public static string ConfirmPasswordCaption => DictionaryPhraseRepository.Current.Get("/Accounts/Register/Confirm Password", "Confirm password");
        public static string PasswordCaption => DictionaryPhraseRepository.Current.Get("/Accounts/Register/Password", "Password");
        public static string NewPasswordCaption => DictionaryPhraseRepository.Current.Get("/Accounts/Register/New Password", "New Password");
        public static string ConfirmPasswordMismatch => DictionaryPhraseRepository.Current.Get("/Accounts/Register/Confirm Password Mismatch", "Your password confirmation does not match. Please enter a new password.");
        public static string MinimumPasswordLength => DictionaryPhraseRepository.Current.Get("/Accounts/Register/Minimum Password Length", "Please enter a password with at lease {1} characters");

    }
}