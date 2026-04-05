using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PrisonEmployeeManagement.Models;
using PrisonEmployeeManagement.Services;

namespace PrisonEmployeeManagement.Controllers
{
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(IEmployeeService employeeService, ILogger<EmployeesController> logger)
        {
            _employeeService = employeeService;
            _logger = logger;
        }

        // GET: Employees
        public async Task<IActionResult> Index(string searchTerm, string department, string status)
        {
            try
            {
                var filterModel = new EmployeeFilterViewModel
                {
                    SearchTerm = searchTerm,
                    Department = department,
                    EmploymentStatus = status
                };
                
                IEnumerable<Employee> employees;
                
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    employees = await _employeeService.SearchEmployeesAsync(searchTerm);
                }
                else if (!string.IsNullOrWhiteSpace(department))
                {
                    employees = await _employeeService.GetEmployeesByDepartmentAsync(department);
                }
                else if (!string.IsNullOrWhiteSpace(status))
                {
                    employees = await _employeeService.GetEmployeesByStatusAsync(status);
                }
                else
                {
                    employees = await _employeeService.GetAllEmployeesAsync();
                }
                
                filterModel.Employees = employees.Select(e => new EmployeeViewModel
                {
                    Id = e.Id,
                    EmployeeNumber = e.EmployeeNumber,
                    FullName = e.FullName,
                    Position = e.Position,
                    Department = e.Department,
                    Email = e.Email,
                    PhoneNumber = e.PhoneNumber,
                    EmploymentStatus = e.EmploymentStatus,
                    HireDate = e.HireDate,
                    Age = e.Age
                }).ToList();
                
                // Get filter options
                var departments = await _employeeService.GetDistinctDepartmentsAsync();
                var positions = await _employeeService.GetDistinctPositionsAsync();
                
                ViewBag.Departments = new SelectList(departments);
                ViewBag.Positions = new SelectList(positions);
                ViewBag.Statuses = new SelectList(new[] { "Active", "Inactive", "On Leave", "Suspended", "Retired" });
                ViewBag.DepartmentsCount = departments.Count();
                
                // Get statistics
                var stats = await _employeeService.GetEmployeeStatisticsAsync();
                ViewBag.Statistics = stats;
                
                return View(filterModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading employees index");
                TempData["ErrorMessage"] = "An error occurred while loading employees.";
                return View(new EmployeeFilterViewModel());
            }
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Employee ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var employee = await _employeeService.GetEmployeeWithFullHistoryAsync(id.Value);
            if (employee == null)
            {
                TempData["ErrorMessage"] = $"Employee with ID {id} not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            ViewBag.Statuses = new SelectList(new[] { "Active", "Inactive", "On Leave", "Suspended", "Retired" });
            ViewBag.Shifts = new SelectList(new[] { "Day Shift", "Night Shift", "Rotating Shift", "Weekend Shift" });
            ViewBag.Genders = new SelectList(new[] { "Male", "Female", "Other" });
            ViewBag.SecurityLevels = new SelectList(new[] { "Level 1", "Level 2", "Level 3", "Level 4", "Level 5" });
            ViewBag.EmployeeTypes = new SelectList(new[] { "Permanent", "Contract", "Temporary", "Probationary" });
            ViewBag.Ranks = new SelectList(new[] { "Officer", "Senior Officer", "Sergeant", "Lieutenant", "Captain", "Major", "Chief" });
            
            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeeNumber,FirstName,LastName,MiddleName,DateOfBirth,Gender,NationalId,PassportNumber,Position,Department,Rank,HireDate,ConfirmationDate,RetirementDate,Email,PhoneNumber,AlternativePhone,EmergencyContactName,EmergencyContactPhone,EmergencyContactRelationship,Address,City,State,ZipCode,Country,EmploymentStatus,EmployeeType,SecurityClearanceLevel,ClearanceExpiryDate,BadgeNumber,ShiftSchedule,Qualifications,Skills,Languages,MedicalConditions,BloodType,CreatedBy")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _employeeService.CreateEmployeeAsync(employee);
                    TempData["SuccessMessage"] = $"Employee {employee.FullName} created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating employee");
                    ModelState.AddModelError("", "An error occurred while creating the employee.");
                }
            }
            
            // Repopulate dropdowns if validation fails
            ViewBag.Statuses = new SelectList(new[] { "Active", "Inactive", "On Leave", "Suspended", "Retired" }, employee.EmploymentStatus);
            ViewBag.Shifts = new SelectList(new[] { "Day Shift", "Night Shift", "Rotating Shift", "Weekend Shift" }, employee.ShiftSchedule);
            ViewBag.Genders = new SelectList(new[] { "Male", "Female", "Other" }, employee.Gender);
            ViewBag.SecurityLevels = new SelectList(new[] { "Level 1", "Level 2", "Level 3", "Level 4", "Level 5" }, employee.SecurityClearanceLevel);
            ViewBag.EmployeeTypes = new SelectList(new[] { "Permanent", "Contract", "Temporary", "Probationary" }, employee.EmployeeType);
            ViewBag.Ranks = new SelectList(new[] { "Officer", "Senior Officer", "Sergeant", "Lieutenant", "Captain", "Major", "Chief" }, employee.Rank);
            
            return View(employee);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
            {
                if (id == null)
                {
                    TempData["ErrorMessage"] = "Employee ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);
                if (employee == null)
                {
                    TempData["ErrorMessage"] = $"Employee with ID {id} not found.";
                    return RedirectToAction(nameof(Index));
                }
                
                // Populate dropdown lists
                ViewBag.Statuses = new SelectList(new[] { "Active", "Inactive", "On Leave", "Suspended", "Retired" }, employee.EmploymentStatus);
                ViewBag.Shifts = new SelectList(new[] { "Day Shift", "Night Shift", "Rotating Shift", "Weekend Shift" }, employee.ShiftSchedule);
                ViewBag.Genders = new SelectList(new[] { "Male", "Female", "Other" }, employee.Gender);
                ViewBag.SecurityLevels = new SelectList(new[] { "Level 1", "Level 2", "Level 3", "Level 4", "Level 5" }, employee.SecurityClearanceLevel);
                ViewBag.EmployeeTypes = new SelectList(new[] { "Permanent", "Contract", "Temporary", "Probationary" }, employee.EmployeeType);
                ViewBag.Ranks = new SelectList(new[] { "Officer", "Senior Officer", "Sergeant", "Lieutenant", "Captain", "Major", "Chief" }, employee.Rank);
                
                return View(employee);
            }
                
   [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, Employee employee)
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
            await _employeeService.UpdateEmployeeAsync(employee);
            TempData["SuccessMessage"] = "Employee updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error: {ex.Message}");
        }
    }
    
    // Repopulate dropdowns
    ViewBag.Statuses = new SelectList(new[] { "Active", "Inactive", "On Leave", "Suspended", "Retired" }, employee.EmploymentStatus);
    ViewBag.Shifts = new SelectList(new[] { "Day Shift", "Night Shift", "Rotating Shift", "Weekend Shift" }, employee.ShiftSchedule);
    ViewBag.Genders = new SelectList(new[] { "Male", "Female", "Other" }, employee.Gender);
    ViewBag.SecurityLevels = new SelectList(new[] { "Level 1", "Level 2", "Level 3", "Level 4", "Level 5" }, employee.SecurityClearanceLevel);
    
    return View(employee);
}
        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Employee ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);
            if (employee == null)
            {
                TempData["ErrorMessage"] = $"Employee with ID {id} not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                var employeeName = employee?.FullName ?? "Unknown";
                
                var success = await _employeeService.DeleteEmployeeAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Employee {employeeName} deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Employee with ID {id} not found.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee with ID {EmployeeId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the employee. They may have related records that need to be removed first.";
            }
            
            return RedirectToAction(nameof(Index));
        }
        
        // GET: Employees/Export
        public async Task<IActionResult> Export()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            // Implement CSV/Excel export logic here
            TempData["InfoMessage"] = "Export functionality will be implemented soon.";
            return RedirectToAction(nameof(Index));
        }
    }
}