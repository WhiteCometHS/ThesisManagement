using System.ComponentModel.DataAnnotations.Schema;
using DiplomaManagement.Interfaces;

namespace DiplomaManagement.Entities
{
    public class PresentationFile : IFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string Extension { get; set; }
        public string FilePath { get; set; }
        public DateTime Uploaded { get; set; }
        public int? ThesisId { get; set; }
        public virtual Thesis? Thesis { get; set; }
        [Column(TypeName = "nvarchar(24)")]
        public FileStatus FileStatus { get; set; }
    }
}
