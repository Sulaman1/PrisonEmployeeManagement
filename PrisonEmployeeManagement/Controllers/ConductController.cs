using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PrisonEmployeeManagement.Models;
using PrisonEmployeeManagement.Services;

namespace PrisonEmployeeManagement.Controllers
{
    public class ConductController : Controller
    {
        private readonly IConductService _conductService;
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<ConductController> _logger;

        public ConductController(
            IConductService conductService, 
            IEmployeeService employeeService,
            ILogger<ConductController> logger)
        {
            _conductService = conductService;
            _employeeService = employeeService;
            _logger = logger;
        }

        // GET: Conduct/Create
        public async Task<IActionResult> Create(int employeeId)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if (employee == null)
            {
                return NotFound();
            }

            ViewBag.EmployeeName = employee.FullName;
            ViewBag.EmployeeId = employeeId;
            ViewBag.ConductTypes = new SelectList(new[] { "Good", "Bad" });
            ViewBag.GoodCategories = new SelectList(Enum.GetValues(typeof(GoodConductCategory)));
            ViewBag.BadCategories = new SelectList(Enum.GetValues(typeof(BadConductCategory)));
            ViewBag.SeverityLevels = new SelectList(new[] { "Minor", "Moderate", "Serious", "Critical" });
            ViewBag.Statuses = new SelectList(new[] { "Active", "Under Review", "Resolved" });

            return View(new EmployeeConduct { EmployeeId = employeeId, IncidentDate = DateTime.Today });
        }

        // POST: Conduct/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeConduct conduct)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _conductService.AddConductRecordAsync(conduct);
                    TempData["SuccessMessage"] = "Conduct record added successfully!";
                    return RedirectToAction("Details", "Employees", new { id = conduct.EmployeeId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating conduct record");
                    ModelState.AddModelError("", "An error occurred while saving the conduct record.");
                }
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(conduct.EmployeeId);
            ViewBag.EmployeeName = employee?.FullName;
            ViewBag.ConductTypes = new SelectList(new[] { "Good", "Bad" }, conduct.ConductType);
            ViewBag.GoodCategories = new SelectList(Enum.GetValues(typeof(GoodConductCategory)), conduct.Category);
            ViewBag.BadCategories = new SelectList(Enum.GetValues(typeof(BadConductCategory)), conduct.Category);
            ViewBag.SeverityLevels = new SelectList(new[] { "Minor", "Moderate", "Serious", "Critical" }, conduct.Severity);
            ViewBag.Statuses = new SelectList(new[] { "Active", "Under Review", "Resolved" }, conduct.Status);

            return View(conduct);
        }

        // GET: Conduct/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var conduct = await _conductService.GetConductRecordByIdAsync(id);
            if (conduct == null)
            {
                return NotFound();
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(conduct.EmployeeId);
            ViewBag.EmployeeName = employee?.FullName;
            ViewBag.ConductTypes = new SelectList(new[] { "Good", "Bad" }, conduct.ConductType);
            ViewBag.GoodCategories = new SelectList(Enum.GetValues(typeof(GoodConductCategory)), conduct.Category);
            ViewBag.BadCategories = new SelectList(Enum.GetValues(typeof(BadConductCategory)), conduct.Category);
            ViewBag.SeverityLevels = new SelectList(new[] { "Minor", "Moderate", "Serious", "Critical" }, conduct.Severity);
            ViewBag.Statuses = new SelectList(new[] { "Active", "Under Review", "Resolved" }, conduct.Status);

            return View(conduct);
        }

        // POST: Conduct/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeConduct conduct)
        {
            if (id != conduct.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _conductService.UpdateConductRecordAsync(conduct);
                    TempData["SuccessMessage"] = "Conduct record updated successfully!";
                    return RedirectToAction("Details", "Employees", new { id = conduct.EmployeeId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating conduct record");
                    ModelState.AddModelError("", "An error occurred while updating the conduct record.");
                }
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(conduct.EmployeeId);
            ViewBag.EmployeeName = employee?.FullName;
            return View(conduct);
        }

        // GET: Conduct/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var conduct = await _conductService.GetConductRecordByIdAsync(id);
            if (conduct == null)
            {
                return NotFound();
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(conduct.EmployeeId);
            ViewBag.EmployeeName = employee?.FullName;
            return View(conduct);
        }

        // POST: Conduct/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var conduct = await _conductService.GetConductRecordByIdAsync(id);
            if (conduct != null)
            {
                await _conductService.DeleteConductRecordAsync(id);
                TempData["SuccessMessage"] = "Conduct record deleted successfully!";
                return RedirectToAction("Details", "Employees", new { id = conduct.EmployeeId });
            }

            return RedirectToAction("Index", "Employees");
        }

        // POST: Conduct/Expunge/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Expunge(int id, string reason)
        {
            var success = await _conductService.ExpungeConductRecordAsync(id, reason);
            if (success)
            {
                TempData["SuccessMessage"] = "Conduct record expunged successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to expunge conduct record.";
            }

            return RedirectToAction("Details", "Employees", new { id = id });
        }
    }
}