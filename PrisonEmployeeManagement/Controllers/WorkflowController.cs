using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PrisonEmployeeManagement.Data;
using PrisonEmployeeManagement.Models;

namespace PrisonEmployeeManagement.Controllers
{
    public class WorkflowController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<WorkflowController> _logger;

        public WorkflowController(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            ILogger<WorkflowController> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        // GET: Workflow/Send/5
        public async Task<IActionResult> Send(int fileId)
        {
            var file = await _context.EFiles.FindAsync(fileId);
            if (file == null)
            {
                return NotFound();
            }

            ViewBag.FileId = fileId;
            ViewBag.FileNumber = file.FileNumber;
            ViewBag.FileTitle = file.FileTitle;
            
            // Get departments list
            var departments = await _context.Employees
                .Select(e => e.Department)
                .Distinct()
                .ToListAsync();
            
            ViewBag.Departments = new SelectList(departments);
            ViewBag.Priorities = new SelectList(new[] { "Low", "Medium", "High", "Urgent" });
            
            // Get employees for dropdown
            var employees = await _context.Employees
                .Select(e => new { e.Id, Name = e.FullName + " (" + e.Department + ")" })
                .ToListAsync();
            ViewBag.Employees = new SelectList(employees, "Id", "Name");

            // Generate workflow number
            var workflowNumber = $"WF-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
            ViewBag.WorkflowNumber = workflowNumber;

            return View(new FileWorkflow 
            { 
                FileId = fileId, 
                SentDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(7),
                Priority = "Medium",
                Status = "Pending"
            });
        }

        // POST: Workflow/Send
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(FileWorkflow workflow)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    workflow.CreatedAt = DateTime.Now;
                    workflow.UpdatedAt = DateTime.Now;
                    workflow.SentDate = DateTime.Now;
                    
                    _context.FileWorkflows.Add(workflow);
                    await _context.SaveChangesAsync();

                    // Add initial remark
                    var remark = new FileWorkflowRemark
                    {
                        WorkflowId = workflow.Id,
                        EmployeeId = workflow.FromEmployeeId,
                        Remark = $"File sent from {workflow.FromDepartment} to {workflow.ToDepartment}. Subject: {workflow.Subject}",
                        ActionType = "Send",
                        CreatedAt = DateTime.Now
                    };
                    _context.FileWorkflowRemarks.Add(remark);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"File sent successfully! Workflow Number: {workflow.WorkflowNumber}";
                    
                    if (workflow.ToEmployeeId.HasValue)
                    {
                        return RedirectToAction("Inbox", new { employeeId = workflow.ToEmployeeId.Value });
                    }
                    return RedirectToAction("Index", "EFiles");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending file");
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            // Repopulate dropdowns
            var departments = await _context.Employees.Select(e => e.Department).Distinct().ToListAsync();
            ViewBag.Departments = new SelectList(departments, workflow.FromDepartment);
            ViewBag.Priorities = new SelectList(new[] { "Low", "Medium", "High", "Urgent" }, workflow.Priority);
            
            var employees = await _context.Employees.Select(e => new { e.Id, Name = e.FullName + " (" + e.Department + ")" }).ToListAsync();
            ViewBag.Employees = new SelectList(employees, "Id", "Name", workflow.ToEmployeeId);
            
            ViewBag.FileId = workflow.FileId;
            var file = await _context.EFiles.FindAsync(workflow.FileId);
            ViewBag.FileNumber = file?.FileNumber;
            ViewBag.FileTitle = file?.FileTitle;
            
            return View(workflow);
        }

        // GET: Workflow/Inbox
        public async Task<IActionResult> Inbox(int? employeeId, string status = "Pending")
        {
            if (employeeId == null)
            {
                var firstEmployee = await _context.Employees.FirstOrDefaultAsync();
                employeeId = firstEmployee?.Id;
            }

            var query = _context.FileWorkflows
                .Include(w => w.File)
                .Include(w => w.FromEmployee)
                .Include(w => w.ToEmployee)
                .Where(w => w.ToEmployeeId == employeeId);

            if (status != "All")
            {
                query = query.Where(w => w.Status == status);
            }

            var workflows = await query
                .OrderByDescending(w => w.IsUrgent)
                .ThenBy(w => w.DueDate)
                .ToListAsync();

            ViewBag.EmployeeId = employeeId;
            ViewBag.CurrentStatus = status;
            ViewBag.Statuses = new SelectList(new[] { "Pending", "In Progress", "Approved", "Rejected", "Completed", "All" }, status);

            return View(workflows);
        }

        // GET: Workflow/Sent
        public async Task<IActionResult> Sent(int? employeeId)
        {
            if (employeeId == null)
            {
                var firstEmployee = await _context.Employees.FirstOrDefaultAsync();
                employeeId = firstEmployee?.Id;
            }

            var workflows = await _context.FileWorkflows
                .Include(w => w.File)
                .Include(w => w.FromEmployee)
                .Include(w => w.ToEmployee)
                .Where(w => w.FromEmployeeId == employeeId)
                .OrderByDescending(w => w.SentDate)
                .ToListAsync();

            ViewBag.EmployeeId = employeeId;
            return View(workflows);
        }

        // GET: Workflow/Details/5
        public async Task<IActionResult> Details(int id)
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

            var remarks = await _context.FileWorkflowRemarks
                .Include(r => r.Employee)
                .Where(r => r.WorkflowId == id)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            // Get employees for forward dropdown
            var employees = await _context.Employees
                .Select(e => new { e.Id, Name = e.FullName + " (" + e.Department + ")" })
                .ToListAsync();
            ViewBag.Employees = new SelectList(employees, "Id", "Name");

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

        // POST: Workflow/AddRemark
        [HttpPost]
        public async Task<IActionResult> AddRemark(int workflowId, string remark, string actionType, IFormFile? attachment)
        {
            try
            {
                var workflow = await _context.FileWorkflows.FindAsync(workflowId);
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

                // Determine employee ID for remark
                int employeeId;
                if (actionType == "Send" || actionType == "Forward")
                {
                    employeeId = workflow.FromEmployeeId;
                }
                else
                {
                    employeeId = workflow.ToEmployeeId ?? workflow.FromEmployeeId;
                }

                var workflowRemark = new FileWorkflowRemark
                {
                    WorkflowId = workflowId,
                    EmployeeId = employeeId,
                    Remark = remark,
                    ActionType = actionType,
                    AttachmentPath = attachmentPath,
                    CreatedAt = DateTime.Now
                };

                _context.FileWorkflowRemarks.Add(workflowRemark);
                
                // Update workflow status based on action
                switch (actionType)
                {
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
                    case "Receive":
                        workflow.Status = "In Progress";
                        workflow.ReceivedDate = DateTime.Now;
                        break;
                    case "Return":
                        workflow.Status = "Returned";
                        break;
                    case "Forward":
                        workflow.Status = "Forwarded";
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

        // POST: Workflow/Forward
        [HttpPost]
        public async Task<IActionResult> Forward(int workflowId, int toEmployeeId, string remark)
        {
            try
            {
                var workflow = await _context.FileWorkflows
                    .Include(w => w.File)
                    .FirstOrDefaultAsync(w => w.Id == workflowId);
                    
                if (workflow == null)
                {
                    return Json(new { success = false, message = "Workflow not found" });
                }

                var newToEmployee = await _context.Employees.FindAsync(toEmployeeId);
                if (newToEmployee == null)
                {
                    return Json(new { success = false, message = "Target employee not found" });
                }
                
                // Create new workflow for forwarding
                var newWorkflow = new FileWorkflow
                {
                    FileId = workflow.FileId,
                    WorkflowNumber = $"FW-{workflow.WorkflowNumber}-{DateTime.Now:yyyyMMdd}",
                    FromDepartment = workflow.ToDepartment,
                    ToDepartment = newToEmployee.Department,
                    FromEmployeeId = workflow.ToEmployeeId ?? workflow.FromEmployeeId,
                    ToEmployeeId = toEmployeeId,
                    Subject = $"Forwarded: {workflow.Subject}",
                    Description = $"Forwarded from {workflow.ToDepartment} to {newToEmployee.Department}",
                    Status = "Pending",
                    Priority = workflow.Priority,
                    SentDate = DateTime.Now,
                    DueDate = workflow.DueDate,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                
                _context.FileWorkflows.Add(newWorkflow);
                
                // Update original workflow
                workflow.Status = "Forwarded";
                workflow.UpdatedAt = DateTime.Now;
                
                // Add remark to original workflow
                var forwardRemark = new FileWorkflowRemark
                {
                    WorkflowId = workflowId,
                    EmployeeId = workflow.ToEmployeeId ?? workflow.FromEmployeeId,
                    Remark = $"Forwarded to {newToEmployee.FullName} ({newToEmployee.Department}). Remark: {remark}",
                    ActionType = "Forward",
                    CreatedAt = DateTime.Now
                };
                _context.FileWorkflowRemarks.Add(forwardRemark);
                
                await _context.SaveChangesAsync();
                
                return Json(new { success = true, message = "File forwarded successfully", newWorkflowId = newWorkflow.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding file");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Workflow/Track/5
        public async Task<IActionResult> Track(int id)
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

            var remarks = await _context.FileWorkflowRemarks
                .Include(r => r.Employee)
                .Where(r => r.WorkflowId == id)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.Remarks = remarks;
            return View(workflow);
        }

        // GET: Workflow/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var stats = new
            {
                TotalWorkflows = await _context.FileWorkflows.CountAsync(),
                Pending = await _context.FileWorkflows.CountAsync(w => w.Status == "Pending"),
                InProgress = await _context.FileWorkflows.CountAsync(w => w.Status == "In Progress"),
                Completed = await _context.FileWorkflows.CountAsync(w => w.Status == "Completed"),
                Urgent = await _context.FileWorkflows.CountAsync(w => w.IsUrgent && w.Status != "Completed"),
                Overdue = await _context.FileWorkflows.CountAsync(w => w.DueDate < DateTime.Today && w.Status != "Completed")
            };

            var recentWorkflows = await _context.FileWorkflows
                .Include(w => w.File)
                .Include(w => w.FromEmployee)
                .Include(w => w.ToEmployee)
                .OrderByDescending(w => w.SentDate)
                .Take(10)
                .ToListAsync();

            ViewBag.Statistics = stats;
            return View(recentWorkflows);
        }
    }
}