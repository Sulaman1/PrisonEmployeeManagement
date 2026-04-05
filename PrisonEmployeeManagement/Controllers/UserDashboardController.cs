using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrisonEmployeeManagement.Data;
using PrisonEmployeeManagement.Services;
using PrisonEmployeeManagement.Models;

namespace PrisonEmployeeManagement.Controllers
{
    public class UserDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<UserDashboardController> _logger;

        public UserDashboardController(
            ApplicationDbContext context,
            IPermissionService permissionService,
            ILogger<UserDashboardController> logger)
        {
            _context = context;
            _permissionService = permissionService;
            _logger = logger;
        }

// GET: UserDashboard
public async Task<IActionResult> Index(int? userId)
{
    try
    {
        // Get current logged-in user
        var currentUserEmail = User.Identity?.Name;
        var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);
        
        if (userId == null && currentUser != null)
        {
            userId = currentUser.EmployeeId;
        }
        
        if (userId == null)
        {
            var firstEmployee = await _context.Employees.FirstOrDefaultAsync();
            userId = firstEmployee?.Id;
        }

        var employee = await _context.Employees.FindAsync(userId);
        if (employee == null)
        {
            return NotFound();
        }

        // Get workflows where this employee is the receiver
        var receivedWorkflows = await _context.FileWorkflows
            .Include(w => w.File)
            .Include(w => w.FromEmployee)
            .Include(w => w.ToEmployee)
            .Where(w => w.ToEmployeeId == employee.Id)
            .OrderByDescending(w => w.IsUrgent)
            .ThenByDescending(w => w.SentDate)
            .ToListAsync();

        // Get workflows where this employee is the sender
        var sentWorkflows = await _context.FileWorkflows
            .Include(w => w.File)
            .Include(w => w.FromEmployee)
            .Include(w => w.ToEmployee)
            .Where(w => w.FromEmployeeId == employee.Id)
            .OrderByDescending(w => w.SentDate)
            .ToListAsync();

        // Get notifications for this user
        var notifications = await _context.Notifications
            .Where(n => n.UserId == employee.Id)
            .OrderByDescending(n => n.CreatedAt)
            .Take(10)
            .ToListAsync();
        
        var unreadCount = await _context.Notifications
            .CountAsync(n => n.UserId == employee.Id && !n.IsRead);

        // Calculate stats
        var stats = new Dictionary<string, int>
        {
            ["Inbox"] = receivedWorkflows.Count(w => w.Status != "Completed" && w.Status != "Closed"),
            ["Sent"] = sentWorkflows.Count,
            ["Pending"] = receivedWorkflows.Count(w => w.Status == "Pending"),
            ["InProgress"] = receivedWorkflows.Count(w => w.Status == "In Progress"),
            ["Completed"] = receivedWorkflows.Count(w => w.Status == "Completed" || w.Status == "Closed"),
            ["Urgent"] = receivedWorkflows.Count(w => w.IsUrgent && w.Status != "Completed")
        };

        ViewBag.Employee = employee;
        ViewBag.Stats = stats;
        ViewBag.ReceivedWorkflows = receivedWorkflows.Take(5);
        ViewBag.SentWorkflows = sentWorkflows.Take(5);
        ViewBag.Notifications = notifications;
        ViewBag.UnreadCount = unreadCount;

        return View();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error loading dashboard");
        TempData["ErrorMessage"] = "An error occurred while loading the dashboard.";
        return View();
    }
}
        // GET: UserDashboard/MyFiles
        public async Task<IActionResult> MyFiles(int userId)
        {
            var files = await _context.EFiles
                .Where(f => f.EmployeeId == userId)
                .OrderByDescending(f => f.UploadDate)
                .ToListAsync();

            return View(files);
        }

        // GET: UserDashboard/MyWorkflows
        public async Task<IActionResult> MyWorkflows(int userId)
        {
            var workflows = await _permissionService.GetUserRelatedFiles(userId);
            return View(workflows);
        }

        // GET: UserDashboard/WorkflowDetails/5
        public async Task<IActionResult> WorkflowDetails(int id, int userId)
        {
            var workflow = await _context.FileWorkflows
                .Include(w => w.File)
                .Include(w => w.FromEmployee)
                .Include(w => w.ToEmployee)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (workflow == null)
            {
                return NotFound();
            }

            // Check if user has permission to view this workflow
            if (workflow.FromEmployeeId != userId && workflow.ToEmployeeId != userId)
            {
                TempData["ErrorMessage"] = "You don't have permission to view this workflow.";
                return RedirectToAction(nameof(Index), new { userId });
            }

            var remarks = await _context.FileWorkflowRemarks
                .Include(r => r.Employee)
                .Where(r => r.WorkflowId == id)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.Remarks = remarks;
            ViewBag.CurrentUserId = userId;

            return View(workflow);
        }

        // POST: UserDashboard/AddRemark
        [HttpPost]
        public async Task<IActionResult> AddRemark(int workflowId, int userId, string remark, string actionType)
        {
            try
            {
                var workflow = await _context.FileWorkflows.FindAsync(workflowId);
                if (workflow == null)
                {
                    return Json(new { success = false, message = "Workflow not found" });
                }

                // Check permission based on action
                if (actionType == "Approve" || actionType == "Reject")
                {
                    var hasPermission = await _permissionService.HasPermission(userId, "Workflow", "Approve");
                    if (!hasPermission)
                    {
                        return Json(new { success = false, message = "You don't have permission to approve/reject" });
                    }
                }
                    
                var workflowRemark = new FileWorkflowRemark
                {
                    WorkflowId = workflowId,
                    EmployeeId = userId,
                    Remark = remark,
                    ActionType = actionType,
                    CreatedAt = DateTime.Now
                };

                _context.FileWorkflowRemarks.Add(workflowRemark);

                // Update workflow status
                switch (actionType)
                {
                    case "Receive":
                        workflow.Status = "In Progress";
                        workflow.ReceivedDate = DateTime.Now;
                        break;
                    case "Approve":
                        workflow.Status = "Approved";
                        workflow.CompletedDate = DateTime.Now;
                        break;
                    case "Reject":
                        workflow.Status = "Rejected";
                        workflow.CompletedDate = DateTime.Now;
                        break;
                    case "Complete":
                        workflow.Status = "Completed";
                        workflow.CompletedDate = DateTime.Now;
                        break;
                }

                workflow.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Remark added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding remark");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}