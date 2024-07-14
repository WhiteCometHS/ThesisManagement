using DiplomaManagement.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiplomaManagement.Data
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string FirstName { get; set; }
        [StringLength(100)]
        public string LastName { get; set; }
        public int? InstituteId { get; set; }
        public virtual Institute? Institute { get; set; }
        public virtual Director? UserDirector { get; set; }
        public virtual Promoter? UserPromoter { get; set; }
        public virtual Student? UserStudent { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Institute> Institutes { get; set; }
        public DbSet<Director> Directors { get; set; }
        public DbSet<Promoter> Promoters { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Thesis> Theses { get; set; }
        public DbSet<PdfFile> PdfFiles { get; set; }
        public DbSet<PresentationFile> PresentationFiles { get; set; }

        public DbSet<Enrollment> Enrollments { get; set; }

        // TODO check if this is neccessary
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(p => p.Institute)
                .WithMany(u => u.Users)
                .HasForeignKey(p => p.InstituteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Director>()
                .HasOne(d => d.User)
                .WithOne(u => u.UserDirector)
                .HasForeignKey<Director>(d => d.DirectorUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Promoter>()
                .HasOne(p => p.User)
                .WithOne(u => u.UserPromoter)
                .HasForeignKey<Promoter>(p => p.PromoterUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Student>()
                .HasOne(p => p.User)
                .WithOne(u => u.UserStudent)
                .HasForeignKey<Student>(p => p.StudentUserId)
                .OnDelete(DeleteBehavior.Cascade);

            /*           modelBuilder.Entity<Director>()
                            .HasOne(p => p.Institute)
                            .WithOne(u => u.Director)
                            .HasForeignKey<Director>(p => p.InstituteId)
                            .OnDelete(DeleteBehavior.Restrict);*/

            modelBuilder.Entity<Promoter>()
                .HasOne(p => p.Director)
                .WithMany(d => d.Promoters) // Указываем, что у Директора может быть много Промоутеров
                .HasForeignKey(p => p.DirectorId) // Устанавливаем внешний ключ в таблице Промоутеров
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Promoter>()
                .HasMany(p => p.Theses)
                .WithOne(t => t.Promoter)
                .HasForeignKey(t => t.PromoterId);

            modelBuilder.Entity<Thesis>()
                .HasMany(p => p.PdfFiles)
                .WithOne(t => t.Thesis)
                .HasForeignKey(t => t.ThesisId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Thesis>()
                .HasOne(t => t.PresentationFile)
                .WithOne()
                .HasForeignKey<Thesis>(t => t.PresentationFileId)
                .OnDelete(DeleteBehavior.Cascade);




            modelBuilder.Entity<Thesis>()
                .HasMany(p => p.Enrollments)
                .WithOne(t => t.Thesis)
                .HasForeignKey(t => t.ThesisId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Thesis)
                .WithMany(t => t.Enrollments)
                .HasForeignKey(e => e.ThesisId)
                .OnDelete(DeleteBehavior.NoAction);


            base.OnModelCreating(modelBuilder);
        }
    }
}
