using DiplomaManagement.Data;
using DiplomaManagement.Entities;
using DiplomaManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DiplomaManagement.Repositories
{
    public class ThesisRepository : IThesisRepository
    {
        private readonly ApplicationDbContext _context;

        public ThesisRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int?> GetStudentThesisAsync(string userId)
        {
            int? thesisId = await _context.Theses
                .Where(t => t.Student != null && t.Student.User != null && t.Student.User.Id == userId)
                .Select(t => (int?)t.Id)
                .FirstOrDefaultAsync();

            return thesisId;
        }

        public async Task<bool> isPromoterHasActiveThesisAsync(string userId)
        {
            bool isAnyActive = await _context.Theses
                .Where(t => t.Promoter != null && t.Promoter.User != null && t.Promoter.User.Id == userId && t.StudentId != null)
                .Select(t => (int?)t.Id)
                .AnyAsync();

            return isAnyActive;
        }

        public async Task assignThesisToStudent(Thesis thesis, int studentId, List<Enrollment> enrollmentsToRemove)
        {
            thesis.Status = ThesisStatus.InProgress;
            thesis.StudentId = studentId;

            _context.Enrollments.RemoveRange(enrollmentsToRemove);
            _context.Update(thesis);
            await _context.SaveChangesAsync();
        }
    }
}