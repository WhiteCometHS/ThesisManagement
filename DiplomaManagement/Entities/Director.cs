using DiplomaManagement.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiplomaManagement.Entities
{
    public class Director
    {
        public int Id { get; set; }

        public string DirectorUserId { get; set; }

        public virtual ApplicationUser? User { get; set; }

/*        public int InstituteId { get; set; }
        public virtual Institute? Institute { get; set; }*/

        public virtual ICollection<Promoter>? Promoters { get; set; }
    }
}
