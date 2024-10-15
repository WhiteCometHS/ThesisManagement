using DiplomaManagement.Entities;

namespace DiplomaManagement.Models
{
        public class ThesisPropositionViewModel
    {
        public int? Id { get; set; }
        public ICollection<PdfFile>? SystemFiles { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public IFormFile? PdfFile { get; set; }
        public int StudentId { get; set; }
        public Student? Student { get; set; }
        public int PromoterId { get; set; }
        public Promoter? Promoter { get; set; }
    }
}