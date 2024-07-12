using DiplomaManagement.Data;
using DiplomaManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiplomaManagement.Repositories
{
    public class InstituteRepository
    {
        private readonly ApplicationDbContext _context;

        public InstituteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Institute>> GetInstitutesWithoutDirector()
        {
            return await _context.Institutes
                .Include(d => d.Users)
                .Where(i => !i.Users.Any(u => u.UserDirector != null))
                .ToListAsync();
        }

        public async Task<List<Institute>> GetAvailableInstitutes()
        {
            return await _context.Institutes
                .Include(d => d.Users)
                .Where(i => !i.Users.Any(u => u.UserDirector != null) && !i.Users.Any(u => u.UserPromoter != null))
                .ToListAsync();
        }
    }
}
