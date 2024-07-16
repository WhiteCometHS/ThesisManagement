using System.ComponentModel;
using System.Data.SqlTypes;
using System.Diagnostics;
using DiplomaManagement.Data;
using DiplomaManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiplomaManagement.Repositories
{
    public class StudentRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int?> GetStudentThesisAsync(string userId)
        {
            var thesisId = await _context.Theses
                .Where(t => t.Student.User.Id == userId)
                .Select(t => (int?)t.Id)
                .FirstOrDefaultAsync();

            return thesisId;
        }
    }
}