using Microsoft.EntityFrameworkCore;
using PrisonEmployeeManagement.Data;
using PrisonEmployeeManagement.Models;
using System.Net.Mail;
using System.Net;

namespace PrisonEmployeeManagement.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Notification> CreateNotification(int userId, string title, string message, string notificationType, int? workflowId = null, int? fileId = null)
        {
            try
            {
                _logger.LogInformation($"Creating notification for user {userId}: {title}");

                var notification = new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    NotificationType = notificationType,
                    WorkflowId = workflowId,
                    FileId = fileId,
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    IsSent = false
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Notification created with ID: {notification.Id}");
                return notification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<NotificationViewModel>> GetUserNotifications(int userId)
        {
            try
            {
                return await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Select(n => new NotificationViewModel
                    {
                        Id = n.Id,
                        Title = n.Title,
                        Message = n.Message,
                        NotificationType = n.NotificationType,
                        IsRead = n.IsRead,
                        CreatedAt = n.CreatedAt,
                        WorkflowId = n.WorkflowId,
                        FileId = n.FileId
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
                return new List<NotificationViewModel>();
            }
        }

        public async Task<List<NotificationViewModel>> GetUnreadNotifications(int userId)
        {
            try
            {
                return await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .OrderByDescending(n => n.CreatedAt)
                    .Select(n => new NotificationViewModel
                    {
                        Id = n.Id,
                        Title = n.Title,
                        Message = n.Message,
                        NotificationType = n.NotificationType,
                        IsRead = n.IsRead,
                        CreatedAt = n.CreatedAt,
                        WorkflowId = n.WorkflowId,
                        FileId = n.FileId
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notifications for user {UserId}", userId);
                return new List<NotificationViewModel>();
            }
        }

        public async Task<int> GetUnreadCount(int userId)
        {
            try
            {
                return await _context.Notifications
                    .CountAsync(n => n.UserId == userId && !n.IsRead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count for user {UserId}", userId);
                return 0;
            }
        }

        public async Task MarkAsRead(int notificationId)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification != null)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Notification {notificationId} marked as read");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
            }
        }

        public async Task MarkAllAsRead(int userId)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.Now;
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation($"All notifications marked as read for user {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
            }
        }

        public async Task DeleteNotification(int notificationId)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification != null)
                {
                    _context.Notifications.Remove(notification);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Notification {notificationId} deleted");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId}", notificationId);
            }
        }

        public async Task SendEmailNotification(string toEmail, string subject, string body)
        {
            try
            {
                // For now, just log - you can configure actual email sending later
                _logger.LogInformation($"Email would be sent to {toEmail}: {subject}");
                // Implement actual email sending if needed
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {ToEmail}", toEmail);
            }
        }
        public async Task NotifyFileForwarded(int toEmployeeId, int fromEmployeeId, int workflowId, string fileName)
        {
            try
            {
                var fromEmployee = await _context.Employees.FindAsync(fromEmployeeId);
                var title = "File Forwarded to You";
                var message = $"{fromEmployee?.FullName ?? "Unknown"} has forwarded the file '{fileName}' to you for further action.";
                
                // Create notification ONLY for the target employee
                await CreateNotification(toEmployeeId, title, message, "FileForwarded", workflowId);
                _logger.LogInformation($"File forwarded notification sent to employee {toEmployeeId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending file forwarded notification");
            }
        }

        public async Task NotifyFileReceived(int toEmployeeId, int fromEmployeeId, int workflowId, string fileName)
        {
            try
            {
                var fromEmployee = await _context.Employees.FindAsync(fromEmployeeId);
                var title = "New File Received";
                var message = $"You have received a new file '{fileName}' from {fromEmployee?.FullName ?? "Unknown"}. Please review and take action.";
                
                // Create notification ONLY for the target employee
                await CreateNotification(toEmployeeId, title, message, "FileReceived", workflowId);
                _logger.LogInformation($"File received notification sent to employee {toEmployeeId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending file received notification");
            }
        }
                
        public async Task NotifyRemarkAdded(int toEmployeeId, int fromEmployeeId, int workflowId, string remark)
        {
            try
            {
                var fromEmployee = await _context.Employees.FindAsync(fromEmployeeId);
                var title = "New Remark Added";
                var message = $"{fromEmployee?.FullName ?? "Unknown"} added a remark on the file: '{remark}'";
                
                await CreateNotification(toEmployeeId, title, message, "RemarkAdded", workflowId);
                _logger.LogInformation($"Remark added notification sent to employee {toEmployeeId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending remark added notification");
            }
        }

        public async Task NotifyFileAction(int toEmployeeId, int fromEmployeeId, int workflowId, string action, string fileName)
        {
            try
            {
                var fromEmployee = await _context.Employees.FindAsync(fromEmployeeId);
                var title = $"File {action}";
                var message = $"{fromEmployee?.FullName ?? "Unknown"} has {action.ToLower()} the file '{fileName}'";
                
                await CreateNotification(toEmployeeId, title, message, action, workflowId);
                _logger.LogInformation($"File action notification sent to employee {toEmployeeId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending file action notification");
            }
        }
    }
}


