using System.Security.Claims;
using DiplomaManagement.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DiplomaManagement.Services
{
    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        private readonly ApplicationDbContext _context;

        public CustomUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            ApplicationDbContext context)
            : base(userManager, roleManager, optionsAccessor)
        {
            _context = context;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            var promoter = await _context.Promoters
                .FirstOrDefaultAsync(p => p.PromoterUserId == user.Id);

            if (promoter != null)
            {
                identity.AddClaim(new Claim("PromoterId", promoter.Id.ToString()));
            }
            else 
            {
                var student = await _context.Students
                    .FirstOrDefaultAsync(p => p.StudentUserId == user.Id);

                if (student != null) 
                {
                    identity.AddClaim(new Claim("StudentId", student.Id.ToString()));
                }
                else 
                {
                    var director = await _context.Directors
                        .FirstOrDefaultAsync(p => p.DirectorUserId == user.Id);

                    if (director != null)
                    {
                        identity.AddClaim(new Claim("DirectorId", director.Id.ToString()));
                    }
                }
            }

            return identity;
        }
    }
}