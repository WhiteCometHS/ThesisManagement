using DiplomaManagement.Entities;

namespace DiplomaManagement.Interfaces
{
    public interface IFile
    {
        int Id { get; set; }
        string FileName { get; set; }
        string FileType { get; set; }
        public string Extension { get; set; }
        string FilePath { get; set; }
        public FileStatus FileStatus { get; set; }
        public int? ThesisId { get; set; }
        public Thesis? Thesis { get; set; }
    }
}
