namespace DiplomaManagement.Interfaces
{
    public interface IThesisPropositionRepository
    {
        Task<int> isPromoterHasThesisPropositionsAsync(int promoterId);
    }
}