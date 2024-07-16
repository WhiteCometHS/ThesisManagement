using DiplomaManagement.Entities;

namespace DiplomaManagement.Models
{
    public class PromoterThesisViewModel
    {
        public int? Id { get; set; }
        public ICollection<PdfFile>? SystemFiles { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int PromoterId { get; set; }
        public IFormFile? PdfFile { get; set; }
    }
}
