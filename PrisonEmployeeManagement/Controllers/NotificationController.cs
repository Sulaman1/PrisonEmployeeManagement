using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrisonEmployeeManagement.Services;

namespace PrisonEmployeeManagement.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        // GET: Notification/Index
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentEmployeeId();
            var notifications = await _notificationService.GetUserNotifications(userId);
            return View(notifications);
        }

        // GET: Notification/UnreadCount
        public async Task<IActionResult> UnreadCount()
        {
            var userId = GetCurrentEmployeeId();
            var count = await _notificationService.GetUnreadCount(userId);
            return Json(new { count = count });
        }

        // POST: Notification/MarkAsRead/5
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsRead(id);
            return Json(new { success = true });
        }

        // POST: Notification/MarkAllAsRead
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentEmployeeId();
            await _notificationService.MarkAllAsRead(userId);
            return Json(new { success = true });
        }

        // POST: Notification/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _notificationService.DeleteNotification(id);
            return Json(new { success = true });
        }

        private int GetCurrentEmployeeId()
        {
            // Get current logged-in user's employee ID
            var userEmail = User.Identity?.Name;
            // This should be implemented based on your authentication
            return 1; // Placeholder
        }
    }
}