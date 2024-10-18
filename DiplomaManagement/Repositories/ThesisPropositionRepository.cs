using DiplomaManagement.Data;
using DiplomaManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DiplomaManagement.Repositories
{
    public class ThesisPropositionRepository : IThesisPropositionRepository
    {
        private readonly ApplicationDbContext _context;

        public ThesisPropositionRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<int> isPromoterHasThesisPropositionsAsync(int promoterId)
        {
            int propositionsCount = await _context.ThesisPropositions
                .Where(t => t.PromoterId == promoterId)
                .CountAsync();

            return propositionsCount;
        }
    }
}