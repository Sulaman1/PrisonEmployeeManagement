using Microsoft.EntityFrameworkCore;
using PrisonEmployeeManagement.Data;
using PrisonEmployeeManagement.Models;

namespace PrisonEmployeeManagement.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(ApplicationDbContext context, ILogger<EmployeeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            try
            {
                return await _context.Employees
                    .Include(e => e.ConductRecords)
                    .Include(e => e.Postings)
                    .Include(e => e.Trainings)
                    .Include(e => e.Awards)
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all employees");
                throw;
            }
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            try
            {
                return await _context.Employees
                    .Include(e => e.ConductRecords)
                    .Include(e => e.Postings)
                    .Include(e => e.Trainings)
                    .Include(e => e.Awards)
                    .Include(e => e.Leaves)
                    .FirstOrDefaultAsync(e => e.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employee with ID {Id}", id);
                throw;
            }
        }

        public async Task<Employee?> GetEmployeeWithFullHistoryAsync(int id)
        {
            try
            {
                return await _context.Employees
                    .Include(e => e.ConductRecords.OrderByDescending(c => c.IncidentDate))
                    .Include(e => e.Postings.OrderByDescending(p => p.StartDate))
                    .Include(e => e.Trainings.OrderByDescending(t => t.StartDate))
                    .Include(e => e.Awards.OrderByDescending(a => a.AwardDate))
                    .Include(e => e.Leaves.OrderByDescending(l => l.StartDate))
                    .FirstOrDefaultAsync(e => e.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employee with full history for ID {Id}", id);
                throw;
            }
        }

        public async Task<Employee> CreateEmployeeAsync(Employee employee)
        {
            try
            {
                employee.CreatedAt = DateTime.Now;
                employee.UpdatedAt = DateTime.Now;
                
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Created new employee with ID {EmployeeId}", employee.Id);
                return employee;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating employee");
                if (ex.InnerException?.Message.Contains("UNIQUE") == true)
                {
                    throw new InvalidOperationException("Employee number, email, or badge number already exists.");
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee");
                throw;
            }
        }

   public async Task<Employee> UpdateEmployeeAsync(Employee employee)
{
    try
    {
        employee.UpdatedAt = DateTime.Now;
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
        return employee;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating employee");
        throw new Exception("Failed to update employee. Please try again.");
    }
}     
        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee == null)
                    return false;
                    
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Deleted employee with ID {EmployeeId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee with ID {EmployeeId}", id);
                throw;
            }
        }

        public async Task<bool> EmployeeExistsAsync(int id)
        {
            return await _context.Employees.AnyAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Employee>> SearchEmployeesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllEmployeesAsync();
                
            return await _context.Employees
                .Include(e => e.ConductRecords)
                .Include(e => e.Postings)
                .Where(e => e.FirstName.Contains(searchTerm) ||
                           e.LastName.Contains(searchTerm) ||
                           e.EmployeeNumber.Contains(searchTerm) ||
                           e.Email.Contains(searchTerm) ||
                           e.Position.Contains(searchTerm) ||
                           e.Department.Contains(searchTerm) ||
                           e.BadgeNumber.Contains(searchTerm))
                .OrderBy(e => e.LastName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByDepartmentAsync(string department)
        {
            if (string.IsNullOrWhiteSpace(department))
                return await GetAllEmployeesAsync();
                
            return await _context.Employees
                .Include(e => e.ConductRecords)
                .Include(e => e.Postings)
                .Where(e => e.Department == department)
                .OrderBy(e => e.LastName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByStatusAsync(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return await GetAllEmployeesAsync();
                
            return await _context.Employees
                .Where(e => e.EmploymentStatus == status)
                .OrderBy(e => e.LastName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByRankAsync(string rank)
        {
            if (string.IsNullOrWhiteSpace(rank))
                return await GetAllEmployeesAsync();
                
            return await _context.Employees
                .Where(e => e.Rank == rank)
                .OrderBy(e => e.LastName)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetDistinctDepartmentsAsync()
        {
            return await _context.Employees
                .Select(e => e.Department)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetDistinctPositionsAsync()
        {
            return await _context.Employees
                .Select(e => e.Position)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetDistinctRanksAsync()
        {
            return await _context.Employees
                .Where(e => e.Rank != null)
                .Select(e => e.Rank!)
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetEmployeeStatisticsAsync()
        {
            var stats = new Dictionary<string, int>();
            
            stats["Total"] = await _context.Employees.CountAsync();
            stats["Active"] = await _context.Employees.CountAsync(e => e.EmploymentStatus == "Active");
            stats["OnLeave"] = await _context.Employees.CountAsync(e => e.EmploymentStatus == "On Leave");
            stats["Inactive"] = await _context.Employees.CountAsync(e => e.EmploymentStatus == "Inactive");
            stats["Suspended"] = await _context.Employees.CountAsync(e => e.EmploymentStatus == "Suspended");
            
            return stats;
        }

        public async Task<IEnumerable<EmployeeConduct>> GetEmployeeConductHistoryAsync(int employeeId)
        {
            return await _context.EmployeeConducts
                .Where(c => c.EmployeeId == employeeId)
                .OrderByDescending(c => c.IncidentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmployeePosting>> GetEmployeePostingHistoryAsync(int employeeId)
        {
            return await _context.EmployeePostings
                .Where(p => p.EmployeeId == employeeId)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmployeeTraining>> GetEmployeeTrainingHistoryAsync(int employeeId)
        {
            return await _context.EmployeeTrainings
                .Where(t => t.EmployeeId == employeeId)
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmployeeAward>> GetEmployeeAwardsAsync(int employeeId)
        {
            return await _context.EmployeeAwards
                .Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.AwardDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmployeeLeave>> GetEmployeeLeaveHistoryAsync(int employeeId)
        {
            return await _context.EmployeeLeaves
                .Where(l => l.EmployeeId == employeeId)
                .OrderByDescending(l => l.StartDate)
                .ToListAsync();
        }

        public async Task<double> GetEmployeeAttendanceRateAsync(int employeeId, int year)
        {
            var totalWorkingDays = 260; // Approximate working days in a year
            var leaves = await _context.EmployeeLeaves
                .Where(l => l.EmployeeId == employeeId && 
                           l.StartDate.Year == year && 
                           l.Status == "Approved")
                .SumAsync(l => l.TotalDays);
            
            var attendanceRate = ((totalWorkingDays - leaves) / (double)totalWorkingDays) * 100;
            return Math.Round(attendanceRate, 2);
        }

        public async Task<int> GetEmployeeDisciplinaryCountAsync(int employeeId)
        {
            return await _context.EmployeeConducts
                .CountAsync(c => c.EmployeeId == employeeId && 
                                c.ConductType == "Bad" && 
                                c.Status != "Expunged");
        }

        public async Task<int> GetEmployeeAwardCountAsync(int employeeId)
        {
            return await _context.EmployeeAwards
                .CountAsync(a => a.EmployeeId == employeeId);
        }
    }
}