using DiplomaManagement.Entities;

namespace DiplomaManagement.Interfaces
{
    public interface IThesisRepository
    {
        Task<int?> GetStudentThesisAsync(string userId);
        Task<bool> isPromoterHasActiveThesisAsync(string userId);
        Task assignThesisToStudent(Thesis thesis, int studentId, List<Enrollment> enrollmentsToRemove);
    }
}
