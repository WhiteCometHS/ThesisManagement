using System.ComponentModel.DataAnnotations.Schema;

namespace DiplomaManagement.Entities
{
    public class PdfFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }

        public string FileType { get; set; }

        public string Extension { get; set; }

        public string FilePath { get; set; }

        public DateTime Uploaded { get; set; }

        [Column(TypeName = "nvarchar(24)")]
        public PdfType PdfType { get; set; }

        public int ThesisId { get; set; }

        public virtual Thesis? Thesis { get; set; }

        [Column(TypeName = "nvarchar(24)")]
        public FileStatus? FileStatus { get; set; }
    }

    public enum PdfType
    {
        example,
        original
    }

    public enum FileStatus
    {
        NotVerified,
        NotAccepted,
        Accepted
    }
}
