using DiplomaManagement.Entities;
using System.ComponentModel.DataAnnotations;

namespace DiplomaManagement.Models
{
    public class PromoterViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Thesis Limit")]
        public int? ThesisLimit { get; set; }

        [Required]
        public int DirectorId { get; set; }

        public Director? Director { get; set; }

        [Display(Name = "Is Seminar Leader?")]
        public bool isSeminarLeader { get; set; }
    }
}
