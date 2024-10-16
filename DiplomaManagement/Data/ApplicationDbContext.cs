﻿using DiplomaManagement.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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
        public DbSet<ThesisProposition> ThesisPropositions { get; set; }
        public DbSet<PdfFile> PdfFiles { get; set; }
        public DbSet<PresentationFile> PresentationFiles { get; set; }

        public DbSet<Enrollment> Enrollments { get; set; }

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

            modelBuilder.Entity<Promoter>()
                .HasOne(p => p.Director)
                .WithMany(d => d.Promoters)
                .HasForeignKey(p => p.DirectorId)
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

            modelBuilder.Entity<ThesisProposition>()
                .HasOne(p => p.Student)
                .WithOne(u => u.ThesisProposition)
                .HasForeignKey<ThesisProposition>(p => p.StudentId)
                .OnDelete(DeleteBehavior.NoAction); // needs to be explicitly defined to bypass multiple cascade paths

            modelBuilder.Entity<PdfFile>()
                .HasOne(p => p.ThesisProposition)
                .WithMany(t => t.PdfFiles)
                .HasForeignKey(t => t.ThesisPropositionId);

            modelBuilder.Entity<PresentationFile>()
                .HasOne(p => p.Thesis)
                .WithOne(t => t.PresentationFile)
                .HasForeignKey<PresentationFile>(p => p.ThesisId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Thesis>()
                .HasMany(p => p.Enrollments)
                .WithOne(t => t.Thesis)
                .HasForeignKey(t => t.ThesisId)
                .OnDelete(DeleteBehavior.NoAction); // needs to be explicitly defined to bypass multiple cascade paths

            base.OnModelCreating(modelBuilder);
        }
    }
}
