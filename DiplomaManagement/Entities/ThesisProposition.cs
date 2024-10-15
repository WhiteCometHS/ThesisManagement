namespace DiplomaManagement.Entities
{
    public class ThesisProposition
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public virtual ICollection<PdfFile>? PdfFiles { get; set; }

        public int PromoterId { get; set; }

        public virtual Promoter? Promoter { get; set; }

        public int StudentId { get; set; }

        public virtual Student? Student { get; set; }
    }
}