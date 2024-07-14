namespace DiplomaManagement.Entities
{
    public class Enrollment
    {
        public int Id { get; set; }

        public int ThesisId { get; set; }

        public virtual Thesis? Thesis { get; set; }

        public int StudentId { get; set; }

        public virtual Student? Student { get; set; }

    }
}
