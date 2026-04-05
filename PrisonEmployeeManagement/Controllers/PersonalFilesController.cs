using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PrisonEmployeeManagement.Data;
using PrisonEmployeeManagement.Models;

namespace PrisonEmployeeManagement.Controllers
{
    [Authorize]
    public class PersonalFilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PersonalFilesController> _logger;

        public PersonalFilesController(ApplicationDbContext context, ILogger<PersonalFilesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: PersonalFiles
        public async Task<IActionResult> Index(string searchTerm, string status = "All")
        {
            try
            {
                var currentEmployee = await GetCurrentEmployee();
                if (currentEmployee == null)
                {
                    TempData["ErrorMessage"] = "Unable to identify current user.";
                    return RedirectToAction("Login", "Account");
                }

                var query = _context.PersonalFiles
                    .Include(p => p.Employee)
                    .Include(p => p.Actions)
                    .Where(p => p.EmployeeId == currentEmployee.Id || p.CreatedBy == currentEmployee.Id);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(p => p.FileNumber.Contains(searchTerm) ||
                                            p.FileName.Contains(searchTerm) ||
                                            p.SentBy.Contains(searchTerm));
                }

                if (status != "All")
                {
                    query = query.Where(p => p.Status == status);
                }

                var files = await query
                    .OrderByDescending(p => p.ReceivedDate)
                    .ToListAsync();

                ViewBag.SearchTerm = searchTerm;
                ViewBag.CurrentStatus = status;
                ViewBag.Statuses = new SelectList(new[] { "All", "Pending", "In Progress", "Approved", "Rejected", "Completed" }, status);
                ViewBag.CurrentEmployeeName = currentEmployee.FullName;

                return View(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading personal files");
                TempData["ErrorMessage"] = "An error occurred while loading files.";
                return View(new List<PersonalFile>());
            }
        }

        // GET: PersonalFiles/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var personalFile = await _context.PersonalFiles
                    .Include(p => p.Employee)
                    .Include(p => p.Actions)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (personalFile == null)
                {
                    return NotFound();
                }

                var currentEmployee = await GetCurrentEmployee();
                if (currentEmployee == null || (personalFile.EmployeeId != currentEmployee.Id && personalFile.CreatedBy != currentEmployee.Id))
                {
                    TempData["ErrorMessage"] = "You don't have permission to view this file.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Statuses = new SelectList(new[] { "Pending", "In Progress", "Approved", "Rejected", "Completed" });

                return View(personalFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading file details");
                TempData["ErrorMessage"] = "An error occurred while loading file details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: PersonalFiles/Create
        public async Task<IActionResult> Create()
        {
            var currentEmployee = await GetCurrentEmployee();
            ViewBag.Employees = new SelectList(await _context.Employees.ToListAsync(), "Id", "FullName");
            ViewBag.Priorities = new SelectList(new[] { "Low", "Medium", "High", "Urgent" });
            ViewBag.Statuses = new SelectList(new[] { "Pending", "In Progress", "Approved", "Rejected", "Completed" });
            
            var fileNumber = $"PF-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
            ViewBag.FileNumber = fileNumber;

            return View(new PersonalFile 
            { 
                ReceivedDate = DateTime.Today,
                DateOfSending = DateTime.Today,
                EmployeeId = currentEmployee?.Id,
                Status = "Pending",
                Priority = "Medium"
            });
        }

        // POST: PersonalFiles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PersonalFile personalFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var currentEmployee = await GetCurrentEmployee();
                    personalFile.CreatedBy = currentEmployee?.Id;
                    personalFile.CreatedAt = DateTime.Now;
                    personalFile.UpdatedAt = DateTime.Now;

                    _context.PersonalFiles.Add(personalFile);
                    await _context.SaveChangesAsync();

                    if (!string.IsNullOrEmpty(personalFile.ActionTaken))
                    {
                        var action = new PersonalFileAction
                        {
                            PersonalFileId = personalFile.Id,
                            ActionDate = DateTime.Today,
                            ActionTaken = personalFile.ActionTaken,
                            ActionBy = currentEmployee?.FullName,
                            Remarks = "Initial action recorded",
                            CreatedAt = DateTime.Now
                        };
                        _context.PersonalFileActions.Add(action);
                        await _context.SaveChangesAsync();
                    }

                    TempData["SuccessMessage"] = "Personal file created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating personal file");
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            ViewBag.Employees = new SelectList(await _context.Employees.ToListAsync(), "Id", "FullName", personalFile.EmployeeId);
            ViewBag.Priorities = new SelectList(new[] { "Low", "Medium", "High", "Urgent" }, personalFile.Priority);
            ViewBag.Statuses = new SelectList(new[] { "Pending", "In Progress", "Approved", "Rejected", "Completed" }, personalFile.Status);
            
            var fileNumber = $"PF-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
            ViewBag.FileNumber = fileNumber;
            
            return View(personalFile);
        }

        // GET: PersonalFiles/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var personalFile = await _context.PersonalFiles.FindAsync(id);
            if (personalFile == null)
            {
                return NotFound();
            }

            var currentEmployee = await GetCurrentEmployee();
            if (personalFile.EmployeeId != currentEmployee?.Id && personalFile.CreatedBy != currentEmployee?.Id)
            {
                TempData["ErrorMessage"] = "You don't have permission to edit this file.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Employees = new SelectList(await _context.Employees.ToListAsync(), "Id", "FullName", personalFile.EmployeeId);
            ViewBag.Priorities = new SelectList(new[] { "Low", "Medium", "High", "Urgent" }, personalFile.Priority);
            ViewBag.Statuses = new SelectList(new[] { "Pending", "In Progress", "Approved", "Rejected", "Completed" }, personalFile.Status);
            
            return View(personalFile);
        }

        // POST: PersonalFiles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PersonalFile personalFile)
        {
            if (id != personalFile.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    personalFile.UpdatedAt = DateTime.Now;
                    _context.Entry(personalFile).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Personal file updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.PersonalFiles.Any(e => e.Id == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating personal file");
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            ViewBag.Employees = new SelectList(await _context.Employees.ToListAsync(), "Id", "FullName", personalFile.EmployeeId);
            ViewBag.Priorities = new SelectList(new[] { "Low", "Medium", "High", "Urgent" }, personalFile.Priority);
            ViewBag.Statuses = new SelectList(new[] { "Pending", "In Progress", "Approved", "Rejected", "Completed" }, personalFile.Status);
            
            return View(personalFile);
        }

        // GET: PersonalFiles/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var personalFile = await _context.PersonalFiles
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (personalFile == null)
            {
                return NotFound();
            }

            return View(personalFile);
        }

        // POST: PersonalFiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var personalFile = await _context.PersonalFiles.FindAsync(id);
            if (personalFile != null)
            {
                _context.PersonalFiles.Remove(personalFile);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Personal file deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: PersonalFiles/AddAction/5
        public async Task<IActionResult> AddAction(int id)
        {
            if (id == 0)
            {
                TempData["ErrorMessage"] = "Invalid file ID";
                return RedirectToAction(nameof(Index));
            }
            
            var personalFile = await _context.PersonalFiles.FindAsync(id);
            if (personalFile == null)
            {
                TempData["ErrorMessage"] = "Personal file not found";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.PersonalFileId = id;
            ViewBag.FileName = personalFile.FileName;
            ViewBag.Statuses = new SelectList(new[] { "Pending", "In Progress", "Approved", "Rejected", "Completed" });
            
            var action = new PersonalFileAction 
            { 
                PersonalFileId = id, 
                ActionDate = DateTime.Today 
            };
            
            return View(action);
        }

        // POST: PersonalFiles/AddAction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAction(int PersonalFileId, string ActionTaken, DateTime ActionDate, string ActionBy, string Remarks, DateTime? NextActionDate, string StatusAfterAction)
        {
            try
            {
                // Log the incoming data for debugging
                _logger.LogInformation($"AddAction called - PersonalFileId: {PersonalFileId}, ActionTaken: {ActionTaken}");
                
                // Validate
                if (PersonalFileId == 0)
                {
                    TempData["ErrorMessage"] = "Invalid file ID. Please try again.";
                    return RedirectToAction(nameof(Index));
                }
                
                if (string.IsNullOrEmpty(ActionTaken))
                {
                    TempData["ErrorMessage"] = "Action Taken is required";
                    return RedirectToAction(nameof(Details), new { id = PersonalFileId });
                }
                
                // Verify the PersonalFile exists
                var existingFile = await _context.PersonalFiles.FindAsync(PersonalFileId);
                if (existingFile == null)
                {
                    TempData["ErrorMessage"] = $"Personal file with ID {PersonalFileId} not found";
                    return RedirectToAction(nameof(Index));
                }
                
                // Get current employee
                var currentEmployee = await GetCurrentEmployee();
                
                // Create new action
                var action = new PersonalFileAction
                {
                    PersonalFileId = PersonalFileId,
                    ActionDate = ActionDate == DateTime.MinValue ? DateTime.Today : ActionDate,
                    ActionTaken = ActionTaken,
                    ActionBy = !string.IsNullOrEmpty(ActionBy) ? ActionBy : (currentEmployee?.FullName ?? User.Identity?.Name ?? "System"),
                    Remarks = Remarks,
                    NextActionDate = NextActionDate,
                    StatusAfterAction = StatusAfterAction,
                    CreatedAt = DateTime.Now
                };
                
                // Add to database
                _context.PersonalFileActions.Add(action);
                int result = await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Action saved. Rows affected: {result}, Action ID: {action.Id}");
                
                // Update main file status if needed
                if (!string.IsNullOrEmpty(StatusAfterAction))
                {
                    existingFile.Status = StatusAfterAction;
                    existingFile.UpdatedAt = DateTime.Now;
                    
                    if (StatusAfterAction == "Completed" || StatusAfterAction == "Approved")
                    {
                        existingFile.FinalDecisionBy = action.ActionBy;
                        existingFile.FinalDecisionDate = action.ActionDate;
                        existingFile.FinalDecision = ActionTaken;
                    }
                    
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"File status updated to: {StatusAfterAction}");
                }
                
                TempData["SuccessMessage"] = "Action added successfully!";
                return RedirectToAction(nameof(Details), new { id = PersonalFileId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding action");
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id = PersonalFileId });
            }
        }

        // GET: PersonalFiles/EditAction/5
        public async Task<IActionResult> EditAction(int id)
        {
            var action = await _context.PersonalFileActions.FindAsync(id);
            if (action == null)
            {
                return NotFound();
            }

            ViewBag.Statuses = new SelectList(new[] { "Pending", "In Progress", "Approved", "Rejected", "Completed" }, action.StatusAfterAction);
            return View(action);
        }

        // POST: PersonalFiles/EditAction/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAction(int id, PersonalFileAction action)
        {
            if (id != action.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Entry(action).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Action updated successfully!";
                    return RedirectToAction(nameof(Details), new { id = action.PersonalFileId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating action");
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            ViewBag.Statuses = new SelectList(new[] { "Pending", "In Progress", "Approved", "Rejected", "Completed" }, action.StatusAfterAction);
            return View(action);
        }

        // POST: PersonalFiles/DeleteAction/5
        [HttpPost]
        public async Task<IActionResult> DeleteAction(int id)
        {
            var action = await _context.PersonalFileActions.FindAsync(id);
            if (action != null)
            {
                int fileId = action.PersonalFileId;
                _context.PersonalFileActions.Remove(action);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Action deleted successfully!";
                return RedirectToAction(nameof(Details), new { id = fileId });
            }
            return RedirectToAction(nameof(Index));
        }

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


