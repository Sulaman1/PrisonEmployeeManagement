using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrisonEmployeeManagement.Data;
using PrisonEmployeeManagement.Models;

namespace PrisonEmployeeManagement.Controllers
{
    public class DebugController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DebugController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var result = new Dictionary<string, object>();
            
            // Get all employees with their user accounts
            var employees = await _context.Employees.ToListAsync();
            var users = await _context.Users.ToListAsync();
            
            result["Employees"] = employees.Select(e => new
            {
                e.Id,
                e.EmployeeNumber,
                e.FullName,
                e.Email,
                HasUserAccount = users.Any(u => u.EmployeeId == e.Id || u.Email == e.Email)
            });
            
            result["Users"] = users.Select(u => new
            {
                u.Id,
                u.Email,
                u.EmployeeId,
                u.FirstName,
                u.LastName
            });
            
            // Get notifications
            var notifications = await _context.Notifications
                .Include(n => n.User)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .ToListAsync();
            
            result["RecentNotifications"] = notifications.Select(n => new
            {
                n.Id,
                n.Title,
                n.Message,
                n.NotificationType,
                n.IsRead,
                n.CreatedAt,
                UserName = n.User?.FullName,
                UserEmail = n.User?.Email
            });
            
            return Json(result);
        }
        
        // Create test notification for specific employee
        [HttpPost]
        public async Task<IActionResult> CreateTestNotification(string email, string title, string message)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
            if (employee == null)
            {
                return Json(new { success = false, message = $"Employee with email {email} not found" });
            }
            
            var notification = new Notification
            {
                UserId = employee.Id,
                Title = title ?? "Test Notification",
                Message = message ?? "This is a test notification",
                NotificationType = "Test",
                CreatedAt = DateTime.Now,
                IsRead = false
            };
            
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            
            return Json(new { success = true, message = $"Notification sent to {employee.FullName} (ID: {employee.Id})" });
        }
    }
}