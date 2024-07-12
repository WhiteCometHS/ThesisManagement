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

        public int? PresentationFileId { get; set; } 

        public virtual PresentationFile? PresentationFile { get; set; }

        public virtual ThesisStatus Status { get; set; }
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
