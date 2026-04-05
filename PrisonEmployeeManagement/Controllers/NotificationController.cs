using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrisonEmployeeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PrisonEmployeeManagement.Data;
using PrisonEmployeeManagement.Models;
using PrisonEmployeeManagement.Services;


namespace PrisonEmployeeManagement.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(INotificationService notificationService, 
                                      ApplicationDbContext context,
                                      ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _context = context;
            _logger = logger;
        }


// GET: Notification/Index
public async Task<IActionResult> Index()
{
    var currentEmployee = await GetCurrentEmployee();
    if (currentEmployee == null)
    {
        TempData["ErrorMessage"] = "Unable to identify current user.";
        return RedirectToAction("Login", "Account");
    }

    var notifications = await _notificationService.GetUserNotifications(currentEmployee.Id);
    return View(notifications);
}


// GET: Notification/UnreadCount
public async Task<IActionResult> UnreadCount()
{
    var currentEmployee = await GetCurrentEmployee();
    if (currentEmployee == null)
    {
        return Json(new { count = 0 });
    }

    var count = await _notificationService.GetUnreadCount(currentEmployee.Id);
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

        // Helper method to get current employee
        private async Task<Employee?> GetCurrentEmployee()
        {
            try
            {
                var userEmail = User.Identity?.Name;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return null;
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user == null || !user.EmployeeId.HasValue)
                {
                    return null;
                }

                return await _context.Employees.FindAsync(user.EmployeeId.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current employee");
                return null;
            }
        }
                
    }
}