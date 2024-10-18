using System.Security.Claims;

namespace DiplomaManagement.Interfaces
{
    public interface IUserService
    {
        int? getPromoterId(ClaimsPrincipal user);

        int? getStudentId(ClaimsPrincipal user);

        int? getDirectorId(ClaimsPrincipal user);
    }
}