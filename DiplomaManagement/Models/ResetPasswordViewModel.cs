using System.ComponentModel.DataAnnotations;

namespace DiplomaManagement.Models
{
    public class ResetPasswordViewModel
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }
    }
}
