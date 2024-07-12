using DiplomaManagement.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiplomaManagement.Entities
{
    public class Institute
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        [StringLength(256, MinimumLength = 3)]
        [Display(Name = "Site")]
        public string SiteAddress { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Street { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string City { get; set; }

        [Required]
        [RegularExpression(@"\d{2}-\d{3}", ErrorMessage = "Invalid postal code format. Correct format is XX-XXX.")]
        [Display(Name = "Post Code")]
        public string PostalCode { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Institute Email")]
        public string Email { get; set; }

        public virtual ICollection<ApplicationUser>? Users { get; set; }
    }
}
