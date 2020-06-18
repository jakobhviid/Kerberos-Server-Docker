using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using app_api.V1.Contracts;

namespace app_api.V1.DTO.InputDTOs.UserDTOs
{
    public class NewUserDTO
    {
        [Required]
        public string AdminPassword { get; set; }
        [Required]
        public string NewUserPassword { get; set; }
        [Required]
        [MaxLength(100)]
        [KerberosUserNameValidation]
        public string NewUserUsername { get; set; }

        public class KerberosUserNameValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var newUserDTO = (NewUserDTO)validationContext.ObjectInstance;
                if (newUserDTO.NewUserUsername == null)
                    return null;
                newUserDTO.NewUserUsername = newUserDTO.NewUserUsername.ToLower();
                var regexItem = new Regex("^[a-zA-Z0-9 ]*$"); // No special characters

                if (!newUserDTO.NewUserUsername.Contains(" ") && regexItem.IsMatch(newUserDTO.NewUserUsername))
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult(ErrorMessages.UserNameValidationError);
                }
            }
        }
    }
}