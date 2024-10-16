using DiplomaManagement.Entities;

namespace DiplomaManagement.Models
{
        public class ThesisPropositionViewModel
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ICollection<IFormFile> PdfFiles { get; set; } = new List<IFormFile>();
        public int StudentId { get; set; }
        public Student? Student { get; set; }
        public int PromoterId { get; set; }
        public Promoter? Promoter { get; set; }
    }
}