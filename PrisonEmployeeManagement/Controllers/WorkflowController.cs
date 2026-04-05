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
    public class WorkflowController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<WorkflowController> _logger;
        private readonly INotificationService _notificationService;

        public WorkflowController(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            ILogger<WorkflowController> logger,
            INotificationService notificationService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _notificationService = notificationService;
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

        // GET: Workflow/Send/5
        [HttpGet]
        public async Task<IActionResult> Send(int fileId)
        {
            try
            {
                var file = await _context.EFiles.FindAsync(fileId);
                if (file == null)
                {
                    TempData["ErrorMessage"] = "File not found.";
                    return RedirectToAction("Index", "EFiles");
                }

                // Get current employee
                var currentEmployee = await GetCurrentEmployee();
                if (currentEmployee == null)
                {
                    TempData["ErrorMessage"] = "Unable to identify current user. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                ViewBag.FileId = fileId;
                ViewBag.FileNumber = file.FileNumber;
                ViewBag.FileTitle = file.FileTitle;
                
                // Get departments list
                var departments = await _context.Employees
                    .Where(e => e.Department != null && !string.IsNullOrEmpty(e.Department))
                    .Select(e => e.Department)
                    .Distinct()
                    .ToListAsync();
                
                ViewBag.Departments = new SelectList(departments);
                ViewBag.Priorities = new SelectList(new[] { "Low", "Medium", "High", "Urgent" });
                
                // Get employees for dropdown (excluding current employee)
                var employees = await _context.Employees
                    .Where(e => e.Id != currentEmployee.Id)
                    .Select(e => new { 
                        e.Id, 
                        Name = (e.FirstName + " " + e.LastName + " (" + (e.Department ?? "Unknown") + ")").Trim() 
                    })
                    .ToListAsync();
                ViewBag.Employees = new SelectList(employees, "Id", "Name");

                // Generate workflow number
                var workflowNumber = $"WF-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
                ViewBag.WorkflowNumber = workflowNumber;

                var workflow = new FileWorkflow 
                { 
                    FileId = fileId, 
                    SentDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(7),
                    Priority = "Medium",
                    Status = "Pending",
                    FromEmployeeId = currentEmployee.Id,
                    FromDepartment = currentEmployee.Department ?? ""
                };

                return View(workflow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading send workflow page");
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index", "EFiles");
            }
        }

        // POST: Workflow/Send
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(FileWorkflow workflow)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate dropdowns
                var departments = await _context.Employees
                    .Where(e => e.Department != null)
                    .Select(e => e.Department)
                    .Distinct()
                    .ToListAsync();
                ViewBag.Departments = new SelectList(departments, workflow.FromDepartment);
                ViewBag.Priorities = new SelectList(new[] { "Low", "Medium", "High", "Urgent" }, workflow.Priority);
                
                var employees = await _context.Employees
                    .Select(e => new { 
                        e.Id, 
                        Name = (e.FirstName + " " + e.LastName + " (" + (e.Department ?? "Unknown") + ")").Trim() 
                    })
                    .ToListAsync();
                ViewBag.Employees = new SelectList(employees, "Id", "Name", workflow.ToEmployeeId);
                
                return View(workflow);
            }

            try
            {
                workflow.CreatedAt = DateTime.Now;
                workflow.UpdatedAt = DateTime.Now;
                workflow.SentDate = DateTime.Now;
                workflow.Status = "Pending";
                
                _context.FileWorkflows.Add(workflow);
                await _context.SaveChangesAsync();

                // Add initial remark
                var remark = new FileWorkflowRemark
                {
                    WorkflowId = workflow.Id,
                    EmployeeId = workflow.FromEmployeeId,
                    Remark = $"File '{workflow.Subject}' sent from {workflow.FromDepartment} to {workflow.ToDepartment}.",
                    ActionType = "Send",
                    CreatedAt = DateTime.Now
                };
                _context.FileWorkflowRemarks.Add(remark);
                await _context.SaveChangesAsync();

                // Send notification to receiving employee
                if (workflow.ToEmployeeId.HasValue)
                {
                    await _notificationService.NotifyFileReceived(
                        workflow.ToEmployeeId.Value, 
                        workflow.FromEmployeeId, 
                        workflow.Id, 
                        workflow.Subject ?? "File"
                    );
                }

                TempData["SuccessMessage"] = $"File sent successfully! Workflow Number: {workflow.WorkflowNumber}";
                return RedirectToAction("Details", new { id = workflow.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending file");
                ModelState.AddModelError("", $"Error: {ex.Message}");
                
                // Repopulate dropdowns
                var departments = await _context.Employees
                    .Where(e => e.Department != null)
                    .Select(e => e.Department)
                    .Distinct()
                    .ToListAsync();
                ViewBag.Departments = new SelectList(departments, workflow.FromDepartment);
                ViewBag.Priorities = new SelectList(new[] { "Low", "Medium", "High", "Urgent" }, workflow.Priority);
                
                var employees = await _context.Employees
                    .Select(e => new { 
                        e.Id, 
                        Name = (e.FirstName + " " + e.LastName + " (" + (e.Department ?? "Unknown") + ")").Trim() 
                    })
                    .ToListAsync();
                ViewBag.Employees = new SelectList(employees, "Id", "Name", workflow.ToEmployeeId);
                
                return View(workflow);
            }
        }

        // GET: Workflow/Inbox
        [HttpGet]
        public async Task<IActionResult> Inbox(string status = "Pending", string searchTerm = "")
        {
            try
            {
                var currentEmployee = await GetCurrentEmployee();
                if (currentEmployee == null)
                {
                    TempData["ErrorMessage"] = "Unable to identify current user.";
                    return RedirectToAction("Login", "Account");
                }

                // Only show workflows where current employee is the receiver
                var query = _context.FileWorkflows
                    .Include(w => w.File)
                    .Include(w => w.FromEmployee)
                    .Include(w => w.ToEmployee)
                    .Where(w => w.ToEmployeeId == currentEmployee.Id);

                if (!string.IsNullOrEmpty(status) && status != "All")
                {
                    query = query.Where(w => w.Status == status);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(w => w.WorkflowNumber.Contains(searchTerm) ||
                                            (w.Subject != null && w.Subject.Contains(searchTerm)) ||
                                            (w.FromEmployee != null && w.FromEmployee.FullName.Contains(searchTerm)));
                }

                var workflows = await query
                    .OrderByDescending(w => w.IsUrgent)
                    .ThenBy(w => w.DueDate)
                    .ThenByDescending(w => w.SentDate)
                    .ToListAsync();

                ViewBag.CurrentStatus = status;
                ViewBag.SearchTerm = searchTerm;
                ViewBag.Statuses = new SelectList(new[] { "Pending", "In Progress", "Approved", "Rejected", "Completed", "Closed", "All" }, status);
                ViewBag.UnreadCount = await _notificationService.GetUnreadCount(currentEmployee.Id);

                return View(workflows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading inbox");
                TempData["ErrorMessage"] = "An error occurred while loading inbox.";
                return View(new List<FileWorkflow>());
            }
        }

        // GET: Workflow/Sent
        [HttpGet]
        public async Task<IActionResult> Sent(string searchTerm = "")
        {
            try
            {
                var currentEmployee = await GetCurrentEmployee();
                if (currentEmployee == null)
                {
                    TempData["ErrorMessage"] = "Unable to identify current user.";
                    return RedirectToAction("Login", "Account");
                }

                // Only show workflows where current employee is the sender
                var query = _context.FileWorkflows
                    .Include(w => w.File)
                    .Include(w => w.FromEmployee)
                    .Include(w => w.ToEmployee)
                    .Where(w => w.FromEmployeeId == currentEmployee.Id);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(w => w.WorkflowNumber.Contains(searchTerm) ||
                                            (w.Subject != null && w.Subject.Contains(searchTerm)) ||
                                            (w.ToEmployee != null && w.ToEmployee.FullName.Contains(searchTerm)));
                }

                var workflows = await query
                    .OrderByDescending(w => w.SentDate)
                    .ToListAsync();

                ViewBag.SearchTerm = searchTerm;
                return View(workflows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sent items");
                TempData["ErrorMessage"] = "An error occurred while loading sent items.";
                return View(new List<FileWorkflow>());
            }
        }

        // GET: Workflow/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var workflow = await _context.FileWorkflows
                    .Include(w => w.File)
                    .Include(w => w.FromEmployee)
                    .Include(w => w.ToEmployee)
                    .FirstOrDefaultAsync(w => w.Id == id);

                if (workflow == null)
                {
                    TempData["ErrorMessage"] = "Workflow not found.";
                    return RedirectToAction("Inbox");
                }

                // Check permission
                var currentEmployee = await GetCurrentEmployee();
                if (currentEmployee == null)
                {
                    TempData["ErrorMessage"] = "Unable to identify current user.";
                    return RedirectToAction("Login", "Account");
                }

                if (workflow.FromEmployeeId != currentEmployee.Id && workflow.ToEmployeeId != currentEmployee.Id)
                {
                    TempData["ErrorMessage"] = "You don't have permission to view this workflow.";
                    return RedirectToAction("Inbox");
                }

                var remarks = await _context.FileWorkflowRemarks
                    .Include(r => r.Employee)
                    .Where(r => r.WorkflowId == id)
                    .OrderBy(r => r.CreatedAt)
                    .ToListAsync();

                // Get employees for forward dropdown
                var employees = await _context.Employees
                    .Where(e => e.Id != currentEmployee.Id)
                    .Select(e => new { 
                        e.Id, 
                        Name = (e.FirstName + " " + e.LastName + " (" + (e.Department ?? "Unknown") + ")").Trim() 
                    })
                    .ToListAsync();
                ViewBag.Employees = new SelectList(employees, "Id", "Name");
                ViewBag.CurrentEmployeeId = currentEmployee.Id;
                ViewBag.CanAct = workflow.ToEmployeeId == currentEmployee.Id && workflow.Status == "Pending";
                ViewBag.CanClose = (workflow.Status == "Approved" || workflow.Status == "Completed") && workflow.Status != "Closed";

                var viewModel = new FileWorkflowViewModel
                {
                    Id = workflow.Id,
                    WorkflowNumber = workflow.WorkflowNumber,
                    FileNumber = workflow.File?.FileNumber ?? "N/A",
                    FileTitle = workflow.File?.FileTitle ?? "N/A",
                    FromDepartment = workflow.FromDepartment,
                    ToDepartment = workflow.ToDepartment,
                    FromEmployee = workflow.FromEmployee?.FullName ?? "N/A",
                    ToEmployee = workflow.ToEmployee?.FullName ?? "N/A",
                    Subject = workflow.Subject ?? "N/A",
                    Status = workflow.Status,
                    Priority = workflow.Priority ?? "Medium",
                    SentDate = workflow.SentDate,
                    ReceivedDate = workflow.ReceivedDate,
                    DueDate = workflow.DueDate,
                    Remarks = remarks.Select(r => new FileWorkflowRemarkViewModel
                    {
                        EmployeeName = r.Employee?.FullName ?? "System",
                        Remark = r.Remark,
                        ActionType = r.ActionType,
                        CreatedAt = r.CreatedAt,
                        AttachmentPath = r.AttachmentPath
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading workflow details");
                TempData["ErrorMessage"] = "An error occurred while loading workflow details.";
                return RedirectToAction("Inbox");
            }
        }

// POST: UserDashboard/AddRemark
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> AddRemark(int workflowId, int userId, string actionType, string remark, IFormFile? attachment)
{
    try
    {
        if (string.IsNullOrEmpty(remark))
        {
            return Json(new { success = false, message = "Remark cannot be empty" });
        }

        if (string.IsNullOrEmpty(actionType))
        {
            return Json(new { success = false, message = "Action type is required" });
        }

        var workflow = await _context.FileWorkflows
            .Include(w => w.FromEmployee)
            .Include(w => w.ToEmployee)
            .FirstOrDefaultAsync(w => w.Id == workflowId);
            
        if (workflow == null)
        {
            return Json(new { success = false, message = "Workflow not found" });
        }

        string? attachmentPath = null;
        if (attachment != null && attachment.Length > 0)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "workflow");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{workflow.WorkflowNumber}_{DateTime.Now:yyyyMMddHHmmss}_{attachment.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await attachment.CopyToAsync(fileStream);
            }
            
            attachmentPath = $"/uploads/workflow/{uniqueFileName}";
        }

        var workflowRemark = new FileWorkflowRemark
        {
            WorkflowId = workflowId,
            EmployeeId = userId,
            Remark = remark,
            ActionType = actionType,
            AttachmentPath = attachmentPath,
            CreatedAt = DateTime.Now
        };

        _context.FileWorkflowRemarks.Add(workflowRemark);
        
        // Update workflow status based on action
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

// GET: UserDashboard/CloseWorkflow/5
public async Task<IActionResult> CloseWorkflow(int id)
{
    try
    {
        var workflow = await _context.FileWorkflows.FindAsync(id);
        if (workflow == null)
        {
            TempData["ErrorMessage"] = "Workflow not found";
            return RedirectToAction(nameof(Index));
        }

        workflow.Status = "Closed";
        workflow.CompletedDate = DateTime.Now;
        workflow.UpdatedAt = DateTime.Now;

        var remark = new FileWorkflowRemark
        {
            WorkflowId = id,
            EmployeeId = workflow.ToEmployeeId ?? workflow.FromEmployeeId,
            Remark = "File closed by user",
            ActionType = "Close",
            CreatedAt = DateTime.Now
        };
        _context.FileWorkflowRemarks.Add(remark);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Workflow closed successfully";
        return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error closing workflow");
        TempData["ErrorMessage"] = "Error closing workflow";
        return RedirectToAction(nameof(Index));
    }
}

        // POST: Workflow/Forward
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Forward(int workflowId, int toEmployeeId, string remark)
        {
            try
            {
                if (toEmployeeId == 0)
                {
                    return Json(new { success = false, message = "Please select an employee to forward to" });
                }

                if (string.IsNullOrEmpty(remark))
                {
                    return Json(new { success = false, message = "Please enter a remark" });
                }

                // Get original workflow
                var originalWorkflow = await _context.FileWorkflows
                    .Include(w => w.File)
                    .Include(w => w.FromEmployee)
                    .Include(w => w.ToEmployee)
                    .FirstOrDefaultAsync(w => w.Id == workflowId);
                    
                if (originalWorkflow == null)
                {
                    return Json(new { success = false, message = "Original workflow not found" });
                }

                // Get current user
                var currentEmployee = await GetCurrentEmployee();
                if (currentEmployee == null)
                {
                    return Json(new { success = false, message = "Unable to identify current user" });
                }

                // Get target employee
                var targetEmployee = await _context.Employees.FindAsync(toEmployeeId);
                if (targetEmployee == null)
                {
                    return Json(new { success = false, message = "Target employee not found" });
                }

                // Add remark to original workflow before forwarding
                var forwardRemark = new FileWorkflowRemark
                {
                    WorkflowId = originalWorkflow.Id,
                    EmployeeId = currentEmployee.Id,
                    Remark = $"Forwarded to {targetEmployee.FullName} ({targetEmployee.Department}). Remark: {remark}",
                    ActionType = "Forward",
                    CreatedAt = DateTime.Now
                };
                _context.FileWorkflowRemarks.Add(forwardRemark);

                // Update original workflow status
                originalWorkflow.Status = "Forwarded";
                originalWorkflow.UpdatedAt = DateTime.Now;
                
                await _context.SaveChangesAsync();

                // Create NEW workflow for forwarding (don't reuse the same FileId in a way that causes conflicts)
                var newWorkflow = new FileWorkflow
                {
                    FileId = originalWorkflow.FileId,
                    WorkflowNumber = $"FWD-{DateTime.Now:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}",
                    FromDepartment = targetEmployee.Department ?? "Unknown",
                    ToDepartment = targetEmployee.Department ?? "Unknown",
                    FromEmployeeId = currentEmployee.Id,
                    ToEmployeeId = toEmployeeId,
                    Subject = $"Forwarded: {originalWorkflow.Subject}",
                    Description = $"Forwarded from {currentEmployee.FullName} to {targetEmployee.FullName}. Original workflow: {originalWorkflow.WorkflowNumber}",
                    Status = "Pending",
                    Priority = originalWorkflow.Priority,
                    SentDate = DateTime.Now,
                    DueDate = originalWorkflow.DueDate,
                    IsUrgent = originalWorkflow.IsUrgent,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                
                _context.FileWorkflows.Add(newWorkflow);
                await _context.SaveChangesAsync();

                // Add initial remark to new workflow
                var newWorkflowRemark = new FileWorkflowRemark
                {
                    WorkflowId = newWorkflow.Id,
                    EmployeeId = currentEmployee.Id,
                    Remark = $"File forwarded to {targetEmployee.FullName}. Original message: {remark}",
                    ActionType = "Send",
                    CreatedAt = DateTime.Now
                };
                _context.FileWorkflowRemarks.Add(newWorkflowRemark);
                await _context.SaveChangesAsync();

                // Send notification to the forwarded employee
                await _notificationService.NotifyFileForwarded(
                    toEmployeeId,
                    currentEmployee.Id,
                    newWorkflow.Id,
                    originalWorkflow.Subject ?? "File"
                );

                return Json(new { 
                    success = true, 
                    message = $"File forwarded successfully to {targetEmployee.FullName}", 
                    newWorkflowId = newWorkflow.Id 
                });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while forwarding file");
                return Json(new { success = false, message = $"Database error: {ex.InnerException?.Message ?? ex.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding file");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
                
        // GET: Workflow/Close/5
        [HttpGet]
        public async Task<IActionResult> Close(int id)
        {
            try
            {
                var workflow = await _context.FileWorkflows
                    .Include(w => w.FromEmployee)
                    .Include(w => w.ToEmployee)
                    .FirstOrDefaultAsync(w => w.Id == id);

                if (workflow == null)
                {
                    TempData["ErrorMessage"] = "Workflow not found.";
                    return RedirectToAction("Inbox");
                }

                var currentEmployee = await GetCurrentEmployee();
                if (currentEmployee == null)
                {
                    TempData["ErrorMessage"] = "Unable to identify current user.";
                    return RedirectToAction("Login", "Account");
                }

                // Check if user has permission to close
                if (workflow.FromEmployeeId != currentEmployee.Id && workflow.ToEmployeeId != currentEmployee.Id)
                {
                    TempData["ErrorMessage"] = "You don't have permission to close this file.";
                    return RedirectToAction("Details", new { id });
                }

                // Check if file can be closed
                if (workflow.Status == "Closed")
                {
                    TempData["ErrorMessage"] = "This file is already closed.";
                    return RedirectToAction("Details", new { id });
                }

                // Add closing remark
                var closeRemark = new FileWorkflowRemark
                {
                    WorkflowId = workflow.Id,
                    EmployeeId = currentEmployee.Id,
                    Remark = "File has been closed and marked as complete.",
                    ActionType = "Close",
                    CreatedAt = DateTime.Now
                };
                _context.FileWorkflowRemarks.Add(closeRemark);

                // Update workflow status
                workflow.Status = "Closed";
                workflow.CompletedDate = DateTime.Now;
                workflow.UpdatedAt = DateTime.Now;
                
                await _context.SaveChangesAsync();

                // Send notifications to both parties
                await _notificationService.NotifyFileAction(
                    workflow.FromEmployeeId,
                    currentEmployee.Id,
                    workflow.Id,
                    "Closed",
                    workflow.Subject ?? "File"
                );
                
                if (workflow.ToEmployeeId.HasValue && workflow.ToEmployeeId != workflow.FromEmployeeId)
                {
                    await _notificationService.NotifyFileAction(
                        workflow.ToEmployeeId.Value,
                        currentEmployee.Id,
                        workflow.Id,
                        "Closed",
                        workflow.Subject ?? "File"
                    );
                }

                TempData["SuccessMessage"] = "File has been closed successfully!";
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing workflow");
                TempData["ErrorMessage"] = "An error occurred while closing the file.";
                return RedirectToAction("Details", new { id });
            }
        }

        // GET: Workflow/Track/5
        [HttpGet]
        public async Task<IActionResult> Track(int id)
        {
            try
            {
                var workflow = await _context.FileWorkflows
                    .Include(w => w.File)
                    .Include(w => w.FromEmployee)
                    .Include(w => w.ToEmployee)
                    .FirstOrDefaultAsync(w => w.Id == id);

                if (workflow == null)
                {
                    TempData["ErrorMessage"] = "Workflow not found.";
                    return RedirectToAction("Inbox");
                }

                var remarks = await _context.FileWorkflowRemarks
                    .Include(r => r.Employee)
                    .Where(r => r.WorkflowId == id)
                    .OrderBy(r => r.CreatedAt)
                    .ToListAsync();

                ViewBag.Remarks = remarks;
                return View(workflow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking workflow");
                TempData["ErrorMessage"] = "An error occurred while tracking the workflow.";
                return RedirectToAction("Inbox");
            }
        }

        // GET: Workflow/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var currentEmployee = await GetCurrentEmployee();
                if (currentEmployee == null)
                {
                    TempData["ErrorMessage"] = "Unable to identify current user.";
                    return RedirectToAction("Login", "Account");
                }

                var stats = new
                {
                    TotalWorkflows = await _context.FileWorkflows
                        .Where(w => w.FromEmployeeId == currentEmployee.Id || w.ToEmployeeId == currentEmployee.Id)
                        .CountAsync(),
                    Pending = await _context.FileWorkflows
                        .CountAsync(w => w.ToEmployeeId == currentEmployee.Id && w.Status == "Pending"),
                    InProgress = await _context.FileWorkflows
                        .CountAsync(w => w.ToEmployeeId == currentEmployee.Id && w.Status == "In Progress"),
                    Completed = await _context.FileWorkflows
                        .CountAsync(w => (w.FromEmployeeId == currentEmployee.Id || w.ToEmployeeId == currentEmployee.Id) && w.Status == "Completed"),
                    Urgent = await _context.FileWorkflows
                        .CountAsync(w => w.ToEmployeeId == currentEmployee.Id && w.IsUrgent && w.Status != "Completed"),
                    Overdue = await _context.FileWorkflows
                        .CountAsync(w => w.ToEmployeeId == currentEmployee.Id && w.DueDate < DateTime.Today && w.Status != "Completed")
                };

                var recentWorkflows = await _context.FileWorkflows
                    .Include(w => w.File)
                    .Include(w => w.FromEmployee)
                    .Include(w => w.ToEmployee)
                    .Where(w => w.FromEmployeeId == currentEmployee.Id || w.ToEmployeeId == currentEmployee.Id)
                    .OrderByDescending(w => w.SentDate)
                    .Take(10)
                    .ToListAsync();

                ViewBag.Statistics = stats;
                ViewBag.UnreadCount = await _notificationService.GetUnreadCount(currentEmployee.Id);
                
                return View(recentWorkflows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading workflow dashboard");
                TempData["ErrorMessage"] = "An error occurred while loading the dashboard.";
                return View(new List<FileWorkflow>());
            }
        }
    }
}