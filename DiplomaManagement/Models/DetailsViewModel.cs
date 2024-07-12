namespace DiplomaManagement.Models
{
    public class DetailsViewModel<TEntity>
    {
        public TEntity Entity { get; set; }
        public ResetPasswordViewModel ResetPasswordViewModel { get; set; }
    }
}
