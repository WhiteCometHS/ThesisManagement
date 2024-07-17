namespace DiplomaManagement.Entities
{
    public class PresentationFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string Extension { get; set; }
        public string FilePath { get; set; }
        public DateTime Uploaded { get; set; }
        public int ThesisId { get; set; }
        public virtual Thesis? Thesis { get; set; }
    }
}
