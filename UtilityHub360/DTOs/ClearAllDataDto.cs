using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class ClearAllDataDto
    {
        [Required(ErrorMessage = "Password is required to confirm this action")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "You must confirm the agreement")]
        [MustBeTrue(ErrorMessage = "You must agree to delete all your data")]
        public bool AgreementConfirmed { get; set; }
    }

    // Custom validation attribute to ensure agreement is true
    public class MustBeTrueAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is bool boolValue)
            {
                return boolValue == true;
            }
            return false;
        }
    }
}


