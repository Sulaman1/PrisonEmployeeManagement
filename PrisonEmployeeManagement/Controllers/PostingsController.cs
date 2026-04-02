using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PrisonEmployeeManagement.Data;
using PrisonEmployeeManagement.Models;

namespace PrisonEmployeeManagement.Controllers
{
    public class PostingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PostingsController> _logger;

        public PostingsController(ApplicationDbContext context, ILogger<PostingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Postings/Index/5
        public async Task<IActionResult> Index(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                return NotFound();
            }

            var postings = await _context.EmployeePostings
                .Where(p => p.EmployeeId == employeeId)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();

            ViewBag.EmployeeId = employeeId;
            ViewBag.EmployeeName = employee.FullName;
            return View(postings);
        }

        // GET: Postings/Create/5
        public async Task<IActionResult> Create(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                return NotFound();
            }

            ViewBag.EmployeeId = employeeId;
            ViewBag.EmployeeName = employee.FullName;
            ViewBag.FacilityTypes = new SelectList(new[] { "Maximum Security", "Medium Security", "Minimum Security", "Administrative", "Training Facility" });
            ViewBag.PostingTypes = new SelectList(new[] { "Permanent", "Temporary", "Acting", "Secondment", "Training" });
            
            return View(new EmployeePosting { EmployeeId = employeeId, StartDate = DateTime.Today, IsCurrent = false });
        }

        // POST: Postings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeePosting posting)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // If this is set as current, update other postings
                    if (posting.IsCurrent)
                    {
                        var currentPostings = await _context.EmployeePostings
                            .Where(p => p.EmployeeId == posting.EmployeeId && p.IsCurrent)
                            .ToListAsync();
                        
                        foreach (var current in currentPostings)
                        {
                            current.IsCurrent = false;
                            current.EndDate = posting.StartDate;
                        }
                    }

                    posting.CreatedAt = DateTime.Now;
                    posting.UpdatedAt = DateTime.Now;
                    
                    _context.EmployeePostings.Add(posting);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Posting record added successfully!";
                    return RedirectToAction(nameof(Index), new { employeeId = posting.EmployeeId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating posting");
                    ModelState.AddModelError("", "Error creating posting record.");
                }
            }

            ViewBag.EmployeeId = posting.EmployeeId;
            ViewBag.FacilityTypes = new SelectList(new[] { "Maximum Security", "Medium Security", "Minimum Security", "Administrative", "Training Facility" }, posting.FacilityType);
            ViewBag.PostingTypes = new SelectList(new[] { "Permanent", "Temporary", "Acting", "Secondment", "Training" }, posting.PostingType);
            
            return View(posting);
        }

        // GET: Postings/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var posting = await _context.EmployeePostings.FindAsync(id);
            if (posting == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(posting.EmployeeId);
            ViewBag.EmployeeName = employee?.FullName;
            ViewBag.FacilityTypes = new SelectList(new[] { "Maximum Security", "Medium Security", "Minimum Security", "Administrative", "Training Facility" }, posting.FacilityType);
            ViewBag.PostingTypes = new SelectList(new[] { "Permanent", "Temporary", "Acting", "Secondment", "Training" }, posting.PostingType);
            
            return View(posting);
        }

        // POST: Postings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeePosting posting)
        {
            if (id != posting.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // If this is set as current, update other postings
                    if (posting.IsCurrent)
                    {
                        var currentPostings = await _context.EmployeePostings
                            .Where(p => p.EmployeeId == posting.EmployeeId && p.IsCurrent && p.Id != posting.Id)
                            .ToListAsync();
                        
                        foreach (var current in currentPostings)
                        {
                            current.IsCurrent = false;
                            current.EndDate = posting.StartDate;
                        }
                    }

                    posting.UpdatedAt = DateTime.Now;
                    _context.Entry(posting).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Posting record updated successfully!";
                    return RedirectToAction(nameof(Index), new { employeeId = posting.EmployeeId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.EmployeePostings.Any(e => e.Id == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating posting");
                    ModelState.AddModelError("", "Error updating posting record.");
                }
            }

            ViewBag.FacilityTypes = new SelectList(new[] { "Maximum Security", "Medium Security", "Minimum Security", "Administrative", "Training Facility" }, posting.FacilityType);
            ViewBag.PostingTypes = new SelectList(new[] { "Permanent", "Temporary", "Acting", "Secondment", "Training" }, posting.PostingType);
            
            return View(posting);
        }

        // GET: Postings/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var posting = await _context.EmployeePostings
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (posting == null)
            {
                return NotFound();
            }

            return View(posting);
        }

        // POST: Postings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var posting = await _context.EmployeePostings.FindAsync(id);
            if (posting != null)
            {
                int employeeId = posting.EmployeeId;
                _context.EmployeePostings.Remove(posting);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Posting record deleted successfully!";
                return RedirectToAction(nameof(Index), new { employeeId = employeeId });
            }
            
            return RedirectToAction("Index", "Employees");
        }
    }
}