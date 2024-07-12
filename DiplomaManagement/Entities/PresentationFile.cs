namespace DiplomaManagement.Entities
{
    public class PresentationFile
    {
        public int Id { get; set; }
        public byte[] Data { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}
