using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PrisonEmployeeManagement.Data;
using PrisonEmployeeManagement.Models;

namespace PrisonEmployeeManagement.Controllers
{
    public class TrainingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TrainingController> _logger;

        public TrainingController(ApplicationDbContext context, ILogger<TrainingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Training/Index/5
        public async Task<IActionResult> Index(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                return NotFound();
            }

            var trainings = await _context.EmployeeTrainings
                .Where(t => t.EmployeeId == employeeId)
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();

            var awards = await _context.EmployeeAwards
                .Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.AwardDate)
                .ToListAsync();

            ViewBag.EmployeeId = employeeId;
            ViewBag.EmployeeName = employee.FullName;
            ViewBag.Trainings = trainings;
            ViewBag.Awards = awards;
            
            return View();
        }

        // TRAINING CRUD Operations
        [HttpGet]
        public async Task<IActionResult> CreateTraining(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return NotFound();
            
            ViewBag.EmployeeId = employeeId;
            ViewBag.EmployeeName = employee.FullName;
            ViewBag.TrainingTypes = new SelectList(new[] { "Mandatory", "Professional Development", "Technical", "Leadership", "Safety", "Compliance" });
            ViewBag.Statuses = new SelectList(new[] { "Completed", "In Progress", "Pending", "Failed" });
            
            return View(new EmployeeTraining { EmployeeId = employeeId, StartDate = DateTime.Today, Status = "Pending" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTraining(EmployeeTraining training)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    training.CreatedAt = DateTime.Now;
                    _context.EmployeeTrainings.Add(training);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Training record added successfully!";
                    return RedirectToAction(nameof(Index), new { employeeId = training.EmployeeId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating training");
                    ModelState.AddModelError("", "Error creating training record.");
                }
            }

            ViewBag.TrainingTypes = new SelectList(new[] { "Mandatory", "Professional Development", "Technical", "Leadership", "Safety", "Compliance" }, training.TrainingType);
            ViewBag.Statuses = new SelectList(new[] { "Completed", "In Progress", "Pending", "Failed" }, training.Status);
            return View(training);
        }

        [HttpGet]
        public async Task<IActionResult> EditTraining(int id)
        {
            var training = await _context.EmployeeTrainings.FindAsync(id);
            if (training == null) return NotFound();
            
            var employee = await _context.Employees.FindAsync(training.EmployeeId);
            ViewBag.EmployeeName = employee?.FullName;
            ViewBag.TrainingTypes = new SelectList(new[] { "Mandatory", "Professional Development", "Technical", "Leadership", "Safety", "Compliance" }, training.TrainingType);
            ViewBag.Statuses = new SelectList(new[] { "Completed", "In Progress", "Pending", "Failed" }, training.Status);
            
            return View(training);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTraining(int id, EmployeeTraining training)
        {
            if (id != training.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Entry(training).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Training record updated successfully!";
                    return RedirectToAction(nameof(Index), new { employeeId = training.EmployeeId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating training");
                    ModelState.AddModelError("", "Error updating training record.");
                }
            }

            ViewBag.TrainingTypes = new SelectList(new[] { "Mandatory", "Professional Development", "Technical", "Leadership", "Safety", "Compliance" }, training.TrainingType);
            ViewBag.Statuses = new SelectList(new[] { "Completed", "In Progress", "Pending", "Failed" }, training.Status);
            return View(training);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTraining(int id)
        {
            var training = await _context.EmployeeTrainings.FindAsync(id);
            if (training != null)
            {
                int employeeId = training.EmployeeId;
                _context.EmployeeTrainings.Remove(training);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Training record deleted successfully!";
                return RedirectToAction(nameof(Index), new { employeeId = employeeId });
            }
            return RedirectToAction("Index", "Employees");
        }

        // AWARDS CRUD Operations
        [HttpGet]
        public async Task<IActionResult> CreateAward(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return NotFound();
            
            ViewBag.EmployeeId = employeeId;
            ViewBag.EmployeeName = employee.FullName;
            ViewBag.AwardCategories = new SelectList(new[] { "Performance", "Bravery", "Leadership", "Service Excellence", "Innovation", "Teamwork" });
            
            return View(new EmployeeAward { EmployeeId = employeeId, AwardDate = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAward(EmployeeAward award)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    award.CreatedAt = DateTime.Now;
                    _context.EmployeeAwards.Add(award);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Award record added successfully!";
                    return RedirectToAction(nameof(Index), new { employeeId = award.EmployeeId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating award");
                    ModelState.AddModelError("", "Error creating award record.");
                }
            }

            ViewBag.AwardCategories = new SelectList(new[] { "Performance", "Bravery", "Leadership", "Service Excellence", "Innovation", "Teamwork" }, award.Category);
            return View(award);
        }

        [HttpGet]
        public async Task<IActionResult> EditAward(int id)
        {
            var award = await _context.EmployeeAwards.FindAsync(id);
            if (award == null) return NotFound();
            
            var employee = await _context.Employees.FindAsync(award.EmployeeId);
            ViewBag.EmployeeName = employee?.FullName;
            ViewBag.AwardCategories = new SelectList(new[] { "Performance", "Bravery", "Leadership", "Service Excellence", "Innovation", "Teamwork" }, award.Category);
            
            return View(award);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAward(int id, EmployeeAward award)
        {
            if (id != award.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Entry(award).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Award record updated successfully!";
                    return RedirectToAction(nameof(Index), new { employeeId = award.EmployeeId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating award");
                    ModelState.AddModelError("", "Error updating award record.");
                }
            }

            ViewBag.AwardCategories = new SelectList(new[] { "Performance", "Bravery", "Leadership", "Service Excellence", "Innovation", "Teamwork" }, award.Category);
            return View(award);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAward(int id)
        {
            var award = await _context.EmployeeAwards.FindAsync(id);
            if (award != null)
            {
                int employeeId = award.EmployeeId;
                _context.EmployeeAwards.Remove(award);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Award record deleted successfully!";
                return RedirectToAction(nameof(Index), new { employeeId = employeeId });
            }
            return RedirectToAction("Index", "Employees");
        }
    }
}