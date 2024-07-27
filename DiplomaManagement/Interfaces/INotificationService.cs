namespace DiplomaManagement.Interfaces
{
    public interface INotificationService
    {
        void AddNotification(string key, string message);
        string GetNotification(string key);
    }
}
