using DiplomaManagement.Data;

namespace DiplomaManagement.Entities
{
    public class Student
    {
        public int Id { get; set; }

        public string StudentUserId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<Enrollment>? Enrollments { get; set; }

        public virtual Thesis? Thesis { get; set; }
        public virtual ThesisProposition? ThesisProposition { get; set; }
    }
}
