using DiplomaManagement.Interfaces;

namespace DiplomaManagement.Services
{
    public class SessionNotificationService : INotificationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionNotificationService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public void AddNotification(string key, string message)
        {
            _httpContextAccessor.HttpContext.Session.SetString(key, message);
            _httpContextAccessor.HttpContext.Session.SetString($"{key}_Expiry", DateTime.Now.AddMinutes(1).ToString());
        }

        public string GetNotification(string key)
        {
            var expiryTimeString = _httpContextAccessor.HttpContext.Session.GetString($"{key}_Expiry");
            if (!string.IsNullOrEmpty(expiryTimeString))
            {
                var expiryTime = DateTime.Parse(expiryTimeString);
                if (expiryTime > DateTime.Now)
                {
                    var message = _httpContextAccessor.HttpContext.Session.GetString(key);
                    ClearNotification(key);
                    return message;
                }
            }
            return null;
        }

        private void ClearNotification(string key)
        {
            _httpContextAccessor.HttpContext.Session.Remove(key);
            _httpContextAccessor.HttpContext.Session.Remove($"{key}_Expiry");
        }
    }
}
