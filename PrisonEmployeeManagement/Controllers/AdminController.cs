using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PrisonEmployeeManagement.Data;
using PrisonEmployeeManagement.Models;

namespace PrisonEmployeeManagement.Controllers
{
    [Authorize(Roles = "System Admin,HR Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Employees
        public async Task<IActionResult> Employees()
        {
            var employees = await _context.Employees
                .OrderBy(e => e.EmployeeNumber)
                .ToListAsync();
            return View(employees);
        }

        // GET: Admin/CreateEmployee
        public IActionResult CreateEmployee()
        {
            ViewBag.Genders = new SelectList(new[] { "Male", "Female", "Other" });
            ViewBag.Statuses = new SelectList(new[] { "Active", "Inactive", "On Leave", "Suspended", "Retired" });
            ViewBag.EmployeeTypes = new SelectList(new[] { "Permanent", "Contract", "Temporary", "Probationary" });
            ViewBag.Ranks = new SelectList(new[] { "Officer", "Senior Officer", "Sergeant", "Lieutenant", "Captain", "Major", "Chief" });
            ViewBag.Departments = new SelectList(new[] { "Security", "Administration", "HR", "IT", "Medical", "Legal", "Maintenance" });
            
            return View();
        }

        // POST: Admin/CreateEmployee
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployee(Employee employee)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Generate employee number if not provided
                    if (string.IsNullOrEmpty(employee.EmployeeNumber))
                    {
                        var lastEmployee = await _context.Employees
                            .OrderByDescending(e => e.Id)
                            .FirstOrDefaultAsync();
                        int nextNumber = (lastEmployee?.Id ?? 0) + 1;
                        employee.EmployeeNumber = $"EMP{nextNumber:D4}";
                    }

                    employee.CreatedAt = DateTime.Now;
                    employee.UpdatedAt = DateTime.Now;
                    
                    _context.Employees.Add(employee);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = $"Employee {employee.FullName} created successfully! Employee Number: {employee.EmployeeNumber}";
                    return RedirectToAction(nameof(Employees));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating employee");
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            ViewBag.Genders = new SelectList(new[] { "Male", "Female", "Other" }, employee.Gender);
            ViewBag.Statuses = new SelectList(new[] { "Active", "Inactive", "On Leave", "Suspended", "Retired" }, employee.EmploymentStatus);
            ViewBag.EmployeeTypes = new SelectList(new[] { "Permanent", "Contract", "Temporary", "Probationary" }, employee.EmployeeType);
            ViewBag.Ranks = new SelectList(new[] { "Officer", "Senior Officer", "Sergeant", "Lieutenant", "Captain", "Major", "Chief" }, employee.Rank);
            ViewBag.Departments = new SelectList(new[] { "Security", "Administration", "HR", "IT", "Medical", "Legal", "Maintenance" }, employee.Department);
            
            return View(employee);
        }

        // GET: Admin/EditEmployee/5
        public async Task<IActionResult> EditEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            ViewBag.Genders = new SelectList(new[] { "Male", "Female", "Other" }, employee.Gender);
            ViewBag.Statuses = new SelectList(new[] { "Active", "Inactive", "On Leave", "Suspended", "Retired" }, employee.EmploymentStatus);
            ViewBag.EmployeeTypes = new SelectList(new[] { "Permanent", "Contract", "Temporary", "Probationary" }, employee.EmployeeType);
            ViewBag.Ranks = new SelectList(new[] { "Officer", "Senior Officer", "Sergeant", "Lieutenant", "Captain", "Major", "Chief" }, employee.Rank);
            ViewBag.Departments = new SelectList(new[] { "Security", "Administration", "HR", "IT", "Medical", "Legal", "Maintenance" }, employee.Department);
            
            return View(employee);
        }

        // POST: Admin/EditEmployee/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmployee(int id, Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    employee.UpdatedAt = DateTime.Now;
                    _context.Entry(employee).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Employee updated successfully!";
                    return RedirectToAction(nameof(Employees));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Employees.Any(e => e.Id == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating employee");
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            ViewBag.Genders = new SelectList(new[] { "Male", "Female", "Other" }, employee.Gender);
            ViewBag.Statuses = new SelectList(new[] { "Active", "Inactive", "On Leave", "Suspended", "Retired" }, employee.EmploymentStatus);
            ViewBag.EmployeeTypes = new SelectList(new[] { "Permanent", "Contract", "Temporary", "Probationary" }, employee.EmployeeType);
            ViewBag.Ranks = new SelectList(new[] { "Officer", "Senior Officer", "Sergeant", "Lieutenant", "Captain", "Major", "Chief" }, employee.Rank);
            ViewBag.Departments = new SelectList(new[] { "Security", "Administration", "HR", "IT", "Medical", "Legal", "Maintenance" }, employee.Department);
            
            return View(employee);
        }

        // POST: Admin/DeleteEmployee/5
        [HttpPost]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Employee deleted successfully!";
            }
            return RedirectToAction(nameof(Employees));
        }
    }
}