namespace DiplomaManagement.Services
{
    public interface INotificationService
    {
        void AddNotification(string key, string message);
        string GetNotification(string key);
    }
}
