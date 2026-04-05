using PrisonEmployeeManagement.Models;

namespace PrisonEmployeeManagement.Services
{
    public interface INotificationService
    {
        Task<Notification> CreateNotification(int userId, string title, string message, string notificationType, int? workflowId = null, int? fileId = null);
        Task<List<NotificationViewModel>> GetUserNotifications(int userId);
        Task<List<NotificationViewModel>> GetUnreadNotifications(int userId);
        Task<int> GetUnreadCount(int userId);
        Task MarkAsRead(int notificationId);
        Task MarkAllAsRead(int userId);
        Task DeleteNotification(int notificationId);
        Task SendEmailNotification(string toEmail, string subject, string body);
        Task NotifyFileReceived(int toEmployeeId, int fromEmployeeId, int workflowId, string fileName);
        Task NotifyFileForwarded(int toEmployeeId, int fromEmployeeId, int workflowId, string fileName);
        Task NotifyRemarkAdded(int toEmployeeId, int fromEmployeeId, int workflowId, string remark);
        Task NotifyFileAction(int toEmployeeId, int fromEmployeeId, int workflowId, string action, string fileName);
    }
}


