using System.Text.RegularExpressions;
using app_api.V1.Contracts;
using System.ComponentModel.DataAnnotations;

namespace app_api.V1.DTO.InputDTOs.UserDTOs
{
    public class NewServiceDTO
    {
        [Required]
        public string AdminPassword { get; set; }
        [Required]
        public string NewServicePassword { get; set; }
        [Required]
        [MaxLength(100)]
        [KerberosServiceNameValidation]
        public string NewServiceName { get; set; }
        [Required]
        public string NewServiceHost { get; set; }

        public class KerberosServiceNameValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var newServiceDTO = (NewServiceDTO)validationContext.ObjectInstance;
                if (newServiceDTO.NewServiceName == null) {
                    return null; // Don't continue
                }
                newServiceDTO.NewServiceName = newServiceDTO.NewServiceName.ToLower();
                
                var regexItem = new Regex("^[a-zA-Z0-9 ]*$"); // No special characters

                if (!newServiceDTO.NewServiceName.Contains(" ") && regexItem.IsMatch(newServiceDTO.NewServiceName))
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult(ErrorMessages.ServiceNameValidationError);
                }
            }
        }
    }
}