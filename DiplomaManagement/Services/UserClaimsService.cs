using System.Security.Claims;
using DiplomaManagement.Interfaces;

namespace DiplomaManagement.Services
{
    public class UserClaimsService : IUserService
    {
        private int? GetClaimId(ClaimsPrincipal user, string claimType)
        {
            var claim = user.Claims.FirstOrDefault(c => c.Type == claimType);
            if (int.TryParse(claim?.Value, out int id))
            {
                return id;
            }
            return null;
        }

        public int? getPromoterId(ClaimsPrincipal user)
        {
            return GetClaimId(user, "PromoterId");
        }

        public int? getStudentId(ClaimsPrincipal user)
        {
            return GetClaimId(user, "StudentId");
        }

        public int? getDirectorId(ClaimsPrincipal user)
        {
            return GetClaimId(user, "DirectorId");
        }
    }
}