using System.ComponentModel.DataAnnotations;

namespace DiplomaManagement.Models
{
    public class RegistrationModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public int InstituteId { get; set; }

        public bool AcceptUserAgreement { get; set; }

        public string? RegistrationInValid {  get; set; }
    }
}
