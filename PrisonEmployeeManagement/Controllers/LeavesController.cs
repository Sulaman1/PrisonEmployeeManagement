using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PrisonEmployeeManagement.Data;
using PrisonEmployeeManagement.Models;

namespace PrisonEmployeeManagement.Controllers
{
    public class LeavesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LeavesController> _logger;

        public LeavesController(ApplicationDbContext context, ILogger<LeavesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Leaves/Index/5
        public async Task<IActionResult> Index(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                return NotFound();
            }

            var leaves = await _context.EmployeeLeaves
                .Where(l => l.EmployeeId == employeeId)
                .OrderByDescending(l => l.StartDate)
                .ToListAsync();

            ViewBag.EmployeeId = employeeId;
            ViewBag.EmployeeName = employee.FullName;
            return View(leaves);
        }

        // GET: Leaves/Create/5
        public async Task<IActionResult> Create(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                return NotFound();
            }

            ViewBag.EmployeeId = employeeId;
            ViewBag.EmployeeName = employee.FullName;
            ViewBag.LeaveTypes = new SelectList(new[] { "Annual", "Sick", "Maternity", "Paternity", "Bereavement", "Unpaid", "Compensatory", "Study" });
            ViewBag.Statuses = new SelectList(new[] { "Pending", "Approved", "Rejected", "Cancelled" });
            
            return View(new EmployeeLeave { EmployeeId = employeeId, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1), Status = "Pending" });
        }

        // POST: Leaves/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeLeave leave)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Calculate total days
                    leave.TotalDays = (leave.EndDate - leave.StartDate).Days + 1;
                    leave.CreatedAt = DateTime.Now;
                    
                    _context.EmployeeLeaves.Add(leave);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Leave record added successfully!";
                    return RedirectToAction(nameof(Index), new { employeeId = leave.EmployeeId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating leave record");
                    ModelState.AddModelError("", "Error creating leave record.");
                }
            }

            ViewBag.LeaveTypes = new SelectList(new[] { "Annual", "Sick", "Maternity", "Paternity", "Bereavement", "Unpaid", "Compensatory", "Study" }, leave.LeaveType);
            ViewBag.Statuses = new SelectList(new[] { "Pending", "Approved", "Rejected", "Cancelled" }, leave.Status);
            return View(leave);
        }

        // GET: Leaves/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var leave = await _context.EmployeeLeaves.FindAsync(id);
            if (leave == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(leave.EmployeeId);
            ViewBag.EmployeeName = employee?.FullName;
            ViewBag.LeaveTypes = new SelectList(new[] { "Annual", "Sick", "Maternity", "Paternity", "Bereavement", "Unpaid", "Compensatory", "Study" }, leave.LeaveType);
            ViewBag.Statuses = new SelectList(new[] { "Pending", "Approved", "Rejected", "Cancelled" }, leave.Status);
            
            return View(leave);
        }

        // POST: Leaves/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeLeave leave)
        {
            if (id != leave.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Recalculate total days
                    leave.TotalDays = (leave.EndDate - leave.StartDate).Days + 1;
                    
                    _context.Entry(leave).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Leave record updated successfully!";
                    return RedirectToAction(nameof(Index), new { employeeId = leave.EmployeeId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.EmployeeLeaves.Any(e => e.Id == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating leave record");
                    ModelState.AddModelError("", "Error updating leave record.");
                }
            }

            ViewBag.LeaveTypes = new SelectList(new[] { "Annual", "Sick", "Maternity", "Paternity", "Bereavement", "Unpaid", "Compensatory", "Study" }, leave.LeaveType);
            ViewBag.Statuses = new SelectList(new[] { "Pending", "Approved", "Rejected", "Cancelled" }, leave.Status);
            return View(leave);
        }

        // GET: Leaves/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var leave = await _context.EmployeeLeaves
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.Id == id);
                
            if (leave == null)
            {
                return NotFound();
            }

            return View(leave);
        }

        // POST: Leaves/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var leave = await _context.EmployeeLeaves.FindAsync(id);
            if (leave != null)
            {
                int employeeId = leave.EmployeeId;
                _context.EmployeeLeaves.Remove(leave);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Leave record deleted successfully!";
                return RedirectToAction(nameof(Index), new { employeeId = employeeId });
            }
            
            return RedirectToAction("Index", "Employees");
        }

        // GET: Leaves/Approve/5
        public async Task<IActionResult> Approve(int id)
        {
            var leave = await _context.EmployeeLeaves.FindAsync(id);
            if (leave != null)
            {
                leave.Status = "Approved";
                leave.ApprovalDate = DateTime.Now;
                leave.ApprovedBy = "Admin"; // You can get from User.Identity
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Leave approved successfully!";
            }
            return RedirectToAction(nameof(Index), new { employeeId = leave?.EmployeeId });
        }

        // GET: Leaves/Reject/5
        public async Task<IActionResult> Reject(int id)
        {
            var leave = await _context.EmployeeLeaves.FindAsync(id);
            if (leave != null)
            {
                leave.Status = "Rejected";
                leave.ApprovalDate = DateTime.Now;
                leave.ApprovedBy = "Admin";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Leave rejected!";
            }
            return RedirectToAction(nameof(Index), new { employeeId = leave?.EmployeeId });
        }
    }
}