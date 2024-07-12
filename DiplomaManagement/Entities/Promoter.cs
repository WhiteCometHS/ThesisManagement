using DiplomaManagement.Data;

namespace DiplomaManagement.Entities
{
    public class Promoter
    {
        public int Id { get; set; }

        public string PromoterUserId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public int ThesisLimit { get; set; } = 3;

        public int DirectorId { get; set; }

        public virtual Director? Director { get; set; }

        public virtual ICollection<Thesis>? Theses { get; set; }
    }
}
