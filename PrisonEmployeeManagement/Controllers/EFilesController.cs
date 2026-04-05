using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PrisonEmployeeManagement.Data;
using PrisonEmployeeManagement.Models;
using Microsoft.AspNetCore.Identity;
using PrisonEmployeeManagement.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace PrisonEmployeeManagement.Controllers
{
    public class EFilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<EFilesController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public EFilesController(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            ILogger<EFilesController> logger,
            UserManager<ApplicationUser> userManager,   
            INotificationService notificationService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _userManager = userManager; 
            _notificationService = notificationService;
        }

        // Helper method to get current user
        private async Task<ApplicationUser?> GetCurrentUser()
        {
            try
            {
                var userEmail = User.Identity?.Name;
                if (string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogWarning("User email is null or empty");
                    return null;
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user == null)
                {
                    _logger.LogWarning($"User not found for email: {userEmail}");
                    return null;
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return null;
            }
        }


        // Helper method to get current employee ID
        private async Task<int?> GetCurrentEmployeeId()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                return user.EmployeeId;
            }
            return null;
        }
      
        // Helper method to get current user name
        private async Task<string> GetCurrentUserName()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                return $"{user.FirstName} {user.LastName}".Trim();
            }
            return "System";
        }


        // GET: EFiles
        public async Task<IActionResult> Index(string searchTerm, string category, string status, int? employeeId)
        {
            try
            {
                // Get current logged-in user and employee
                var currentUser = await GetCurrentUser();
                var currentEmployee = await GetCurrentEmployee();
                
                if (currentEmployee == null)
                {
                    TempData["ErrorMessage"] = "Unable to identify current user. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                // Build query for files where current employee is involved
                var query = _context.EFiles
                    .Include(e => e.Employee)
                    .AsQueryable();

                // Get all workflow IDs where current employee is involved (as sender or receiver)
                var involvedWorkflowIds = await _context.FileWorkflows
                    .Where(w => w.FromEmployeeId == currentEmployee.Id || w.ToEmployeeId == currentEmployee.Id)
                    .Select(w => w.FileId)
                    .Distinct()
                    .ToListAsync();

                // Filter files based on user involvement
                // User can see files if:
                // 1. File is directly assigned to them (EmployeeId matches)
                // 2. File is part of a workflow they are involved in
                // 3. File was uploaded by them (check by email or name)
                
                var userEmail = currentUser?.Email ?? "";
                var userFullName = currentEmployee.FullName;
                
                query = query.Where(e => 
                    e.EmployeeId == currentEmployee.Id ||  // File directly assigned to employee
                    involvedWorkflowIds.Contains(e.Id) ||  // File involved in workflow with employee
                    (e.UploadedBy != null && (e.UploadedBy == userEmail || e.UploadedBy == userFullName)) // File uploaded by current user
                );

                // Apply search filters
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(e => (e.FileNumber != null && e.FileNumber.Contains(searchTerm)) ||
                                            (e.FileTitle != null && e.FileTitle.Contains(searchTerm)) ||
                                            (e.Description != null && e.Description.Contains(searchTerm)));
                }

                if (!string.IsNullOrWhiteSpace(category))
                {
                    query = query.Where(e => e.Category == category);
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(e => e.Status == status);
                }

                if (employeeId.HasValue)
                {
                    query = query.Where(e => e.EmployeeId == employeeId);
                }

                var files = await query
                    .OrderByDescending(e => e.UploadDate)
                    .Select(e => new EFileViewModel
                    {
                        Id = e.Id,
                        FileNumber = e.FileNumber ?? "N/A",
                        FileTitle = e.FileTitle ?? "N/A",
                        Category = e.Category ?? "Uncategorized",
                        FileType = e.FileType ?? "Unknown",
                        EmployeeName = e.Employee != null ? (e.Employee.FullName ?? "N/A") : "N/A",
                        UploadDate = e.UploadDate,
                        Status = e.Status ?? "Active",
                        ConfidentialLevel = e.ConfidentialLevel ?? "Public",
                        FileSize = e.FileSize
                    })
                    .ToListAsync();

                var searchModel = new EFileSearchViewModel
                {
                    SearchTerm = searchTerm,
                    Category = category,
                    Status = status,
                    EmployeeId = employeeId,
                    Files = files
                };

                // Populate filters (only with categories that exist in user's files)
                var userCategories = await query.Select(e => e.Category).Where(c => c != null).Distinct().ToListAsync();
                ViewBag.Categories = new SelectList(userCategories);
                ViewBag.Statuses = new SelectList(new[] { "Active", "Archived" });
                ViewBag.ConfidentialLevels = new SelectList(new[] { "Public", "Internal", "Confidential", "Secret", "Top Secret" });
                
                // Add user info to view
                ViewBag.CurrentEmployeeName = currentEmployee.FullName ?? "Unknown";
                ViewBag.CurrentEmployeeId = currentEmployee.Id;
                ViewBag.FileCount = files.Count;

                return View(searchModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading files index");
                TempData["ErrorMessage"] = $"An error occurred while loading files: {ex.Message}";
                return View(new EFileSearchViewModel());
            }
        }

// GET: EFiles/GetFileDetails/5
[HttpGet]
public async Task<IActionResult> GetFileDetails(int id)
{
    try
    {
        var file = await _context.EFiles
            .Include(e => e.Employee)
            .FirstOrDefaultAsync(e => e.Id == id);
            
        if (file == null)
        {
            return Json(new { success = false, message = "File not found" });
        }
        
        return Json(new
        {
            success = true,
            data = new
            {
                id = file.Id,
                fileNumber = file.FileNumber,
                fileTitle = file.FileTitle,
                description = file.Description,
                category = file.Category,
                fileType = file.FileType,
                fileSize = file.FileSize,
                uploadDate = file.UploadDate,
                uploadedBy = file.UploadedBy,
                status = file.Status,
                confidentialLevel = file.ConfidentialLevel,
                employeeName = file.Employee?.FullName,
                filePath = file.FilePath
            }
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting file details");
        return Json(new { success = false, message = ex.Message });
    }
}

        // GET: EFiles/GetFileContent/5
        [HttpGet]
        public async Task<IActionResult> GetFileContent(int id)
        {
            try
            {
                var file = await _context.EFiles.FindAsync(id);
                if (file == null || string.IsNullOrEmpty(file.FilePath))
                {
                    return NotFound("File not found");
                }
                
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, file.FilePath.TrimStart('/'));
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("File not found on server");
                }
                
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var contentType = GetContentType(file.FileType);
                
                // Log access
                file.LastAccessed = DateTime.Now;
                file.AccessCount++;
                await _context.SaveChangesAsync();
                
                return File(fileBytes, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file content");
                return StatusCode(500, "Error reading file");
            }
        }

        private string GetContentType(string fileType)
        {
            if (string.IsNullOrEmpty(fileType)) return "application/octet-stream";
            
            return fileType.ToLower() switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".txt" => "text/plain",
                ".html" or ".htm" => "text/html",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
            };
        }



        // GET: EFiles/Details/
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var file = await _context.EFiles
                .Include(e => e.Employee)
                .Include(e => e.Conduct)
                .Include(e => e.Posting)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (file == null)
            {
                return NotFound();
            }

            // Log access
            file.LastAccessed = DateTime.Now;
            file.AccessCount++;
            await _context.SaveChangesAsync();

            // Record access log
            var accessLog = new FileAccessLog
            {
                FileId = file.Id,
                AccessedBy = User.Identity?.Name ?? "System",
                AccessTime = DateTime.Now,
                Action = "View",
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString()
            };
            _context.FileAccessLogs.Add(accessLog);
            await _context.SaveChangesAsync();

            // Get access logs for this file
            var accessLogs = await _context.FileAccessLogs
                .Where(l => l.FileId == file.Id)
                .OrderByDescending(l => l.AccessTime)
                .ToListAsync();

            // Pass access logs to view using ViewBag
            ViewBag.AccessLogs = accessLogs;

            return View(file);
        }
        
        // GET: EFiles/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Employees = new SelectList(await _context.Employees.Select(e => new { e.Id, e.FullName }).ToListAsync(), "Id", "FullName");
            ViewBag.Categories = new SelectList(new[] { "Personnel", "Disciplinary", "Training", "Medical", "Contract", "Security", "Incident", "Legal" });
            ViewBag.ConfidentialLevels = new SelectList(new[] { "Public", "Internal", "Confidential", "Secret", "Top Secret" });
            ViewBag.Statuses = new SelectList(new[] { "Active", "Archived" });

            // Generate unique file number
            var fileNumber = $"FILE-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
            ViewBag.FileNumber = fileNumber;

            return View(new EFile { UploadDate = DateTime.Today, Status = "Active" });
        }

        // POST: EFiles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EFile eFile, IFormFile? uploadedFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Handle file upload
                    if (uploadedFile != null && uploadedFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "efiles");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = $"{eFile.FileNumber}_{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(uploadedFile.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await uploadedFile.CopyToAsync(fileStream);
                        }

                        eFile.FilePath = $"/uploads/efiles/{uniqueFileName}";
                        eFile.FileType = Path.GetExtension(uploadedFile.FileName).ToUpper();
                        eFile.FileSize = uploadedFile.Length / 1024;
                    }

                    eFile.CreatedAt = DateTime.Now;
                    eFile.UpdatedAt = DateTime.Now;
                    eFile.UploadDate = DateTime.Now;
                    eFile.UploadedBy = User.Identity?.Name ?? "System";

                    _context.Add(eFile);
                    await _context.SaveChangesAsync();

                    // IMPORTANT: Create workflow if employee is selected
                    if (eFile.EmployeeId.HasValue)
                    {
                        var currentEmployee = await GetCurrentEmployee();
                        var targetEmployee = await _context.Employees.FindAsync(eFile.EmployeeId.Value);
                        
                        if (currentEmployee != null && targetEmployee != null)
                        {
                            // Create workflow for this file
                            var workflow = new FileWorkflow
                            {
                                FileId = eFile.Id,
                                WorkflowNumber = $"WF-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                                FromDepartment = currentEmployee.Department ?? "Unknown",
                                ToDepartment = targetEmployee.Department ?? "Unknown",
                                FromEmployeeId = currentEmployee.Id,
                                ToEmployeeId = targetEmployee.Id,
                                Subject = eFile.FileTitle,
                                Description = eFile.Description ?? "Please review this file.",
                                Status = "Pending",
                                Priority = "Medium",
                                SentDate = DateTime.Now,
                                DueDate = DateTime.Now.AddDays(7),
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            };
                            
                            _context.FileWorkflows.Add(workflow);
                            await _context.SaveChangesAsync();
                            
                            // Add initial remark
                            var remark = new FileWorkflowRemark
                            {
                                WorkflowId = workflow.Id,
                                EmployeeId = currentEmployee.Id,
                                Remark = $"File '{eFile.FileTitle}' has been created and assigned to you for review.",
                                ActionType = "Send",
                                CreatedAt = DateTime.Now
                            };
                            _context.FileWorkflowRemarks.Add(remark);
                            await _context.SaveChangesAsync();
                            
                            // Send notification to target employee
                            if (_notificationService != null)
                            {
                                await _notificationService.NotifyFileReceived(
                                    targetEmployee.Id,
                                    currentEmployee.Id,
                                    workflow.Id,
                                    eFile.FileTitle
                                );
                            }
                            
                            TempData["SuccessMessage"] = $"File uploaded and workflow created! Workflow Number: {workflow.WorkflowNumber}";
                        }
                        else
                        {
                            TempData["SuccessMessage"] = "File uploaded successfully!";
                        }
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "File uploaded successfully!";
                    }
                    
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating file record");
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            // Repopulate dropdowns
            ViewBag.Employees = new SelectList(await _context.Employees.Select(e => new { e.Id, e.FullName, e.Email }).ToListAsync(), "Id", "FullName", eFile.EmployeeId);
            ViewBag.Categories = new SelectList(new[] { "Personnel", "Disciplinary", "Training", "Medical", "Contract", "Security", "Incident", "Legal" }, eFile.Category);
            ViewBag.ConfidentialLevels = new SelectList(new[] { "Public", "Internal", "Confidential", "Secret", "Top Secret" }, eFile.ConfidentialLevel);
            ViewBag.Statuses = new SelectList(new[] { "Active", "Archived" }, eFile.Status);

            return View(eFile);
        }
    
        // Helper method to get current employee
        private async Task<Employee?> GetCurrentEmployee()
        {
            try
            {
                var userEmail = User.Identity?.Name;
                if (string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogWarning("User email is null or empty");
                    return null;
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user == null)
                {
                    _logger.LogWarning($"User not found for email: {userEmail}");
                    return null;
                }

                if (!user.EmployeeId.HasValue)
                {
                    _logger.LogWarning($"User {userEmail} has no EmployeeId");
                    return null;
                }

                var employee = await _context.Employees.FindAsync(user.EmployeeId.Value);
                if (employee == null)
                {
                    _logger.LogWarning($"Employee not found for ID: {user.EmployeeId}");
                    return null;
                }

                return employee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current employee");
                return null;
            }
        }

        // GET: EFiles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eFile = await _context.EFiles.FindAsync(id);
            if (eFile == null)
            {
                return NotFound();
            }

            ViewBag.Employees = new SelectList(await _context.Employees.Select(e => new { e.Id, e.FullName }).ToListAsync(), "Id", "FullName", eFile.EmployeeId);
            ViewBag.Categories = new SelectList(new[] { "Personnel", "Disciplinary", "Training", "Medical", "Contract", "Security", "Incident", "Legal" }, eFile.Category);
            ViewBag.ConfidentialLevels = new SelectList(new[] { "Public", "Internal", "Confidential", "Secret", "Top Secret" }, eFile.ConfidentialLevel);
            ViewBag.Statuses = new SelectList(new[] { "Active", "Archived" }, eFile.Status);

            return View(eFile);
        }

        // POST: EFiles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EFile eFile, IFormFile? uploadedFile)
        {
            if (id != eFile.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle file upload if new file is provided
                    if (uploadedFile != null && uploadedFile.Length > 0)
                    {
                        // Delete old file if exists
                        if (!string.IsNullOrEmpty(eFile.FilePath))
                        {
                            var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, eFile.FilePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Upload new file
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "efiles");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = $"{eFile.FileNumber}_{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(uploadedFile.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await uploadedFile.CopyToAsync(fileStream);
                        }

                        eFile.FilePath = $"/uploads/efiles/{uniqueFileName}";
                        eFile.FileType = Path.GetExtension(uploadedFile.FileName).ToUpper();
                        eFile.FileSize = uploadedFile.Length / 1024;
                    }

                    eFile.UpdatedAt = DateTime.Now;
                    _context.Update(eFile);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "File record updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EFileExists(eFile.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating file record");
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            // Repopulate dropdowns
            ViewBag.Employees = new SelectList(await _context.Employees.Select(e => new { e.Id, e.FullName }).ToListAsync(), "Id", "FullName", eFile.EmployeeId);
            ViewBag.Categories = new SelectList(new[] { "Personnel", "Disciplinary", "Training", "Medical", "Contract", "Security", "Incident", "Legal" }, eFile.Category);
            ViewBag.ConfidentialLevels = new SelectList(new[] { "Public", "Internal", "Confidential", "Secret", "Top Secret" }, eFile.ConfidentialLevel);
            ViewBag.Statuses = new SelectList(new[] { "Active", "Archived" }, eFile.Status);

            return View(eFile);
        }

        // GET: EFiles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eFile = await _context.EFiles
                .Include(e => e.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (eFile == null)
            {
                return NotFound();
            }

            return View(eFile);
        }

        // POST: EFiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eFile = await _context.EFiles.FindAsync(id);
            if (eFile != null)
            {
                // Delete physical file
                if (!string.IsNullOrEmpty(eFile.FilePath))
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, eFile.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.EFiles.Remove(eFile);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "File deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: EFiles/Download/5
        public async Task<IActionResult> Download(int id)
        {
            var eFile = await _context.EFiles.FindAsync(id);
            if (eFile == null || string.IsNullOrEmpty(eFile.FilePath))
            {
                return NotFound();
            }

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, eFile.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found on server.");
            }

            // Log download
            eFile.LastAccessed = DateTime.Now;
            eFile.AccessCount++;
            await _context.SaveChangesAsync();

            var accessLog = new FileAccessLog
            {
                FileId = eFile.Id,
                AccessedBy = User.Identity?.Name ?? "System",
                AccessTime = DateTime.Now,
                Action = "Download",
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString()
            };
            _context.FileAccessLogs.Add(accessLog);
            await _context.SaveChangesAsync();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var fileName = $"{eFile.FileNumber}_{eFile.FileTitle}{eFile.FileType?.ToLower()}";
            return File(fileBytes, "application/octet-stream", fileName);
        }

        // GET: EFiles/AccessLog/5
        public async Task<IActionResult> AccessLog(int id)
        {
            var logs = await _context.FileAccessLogs
                .Where(l => l.FileId == id)
                .OrderByDescending(l => l.AccessTime)
                .ToListAsync();

            ViewBag.FileId = id;
            return PartialView("_AccessLogPartial", logs);
        }

        private bool EFileExists(int id)
        {
            return _context.EFiles.Any(e => e.Id == id);
        }
    }
}