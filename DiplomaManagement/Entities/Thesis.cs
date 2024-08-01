using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DiplomaManagement.Entities
{
    public class Thesis
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int PromoterId { get; set; }

        public virtual Promoter? Promoter { get; set; }

        public virtual ICollection<PdfFile>? PdfFiles { get; set; }

        public virtual PresentationFile? PresentationFile { get; set; }

        [Column(TypeName = "nvarchar(24)")]
        [Display(Name = "Status")]
        public ThesisStatus Status { get; set; }

        public int? StudentId { get; set; }

        public virtual Student? Student { get; set; }

        public string? Comment { get; set; }

        public string? ThesisSophistication { get; set; }

        public virtual ICollection<Enrollment>? Enrollments { get; set; }
    }

    public enum ThesisStatus
    {
        Created,
        Available,
        Requested,
        InProgress,
        Accepted
    }
}
