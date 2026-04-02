using Microsoft.EntityFrameworkCore;
using PrisonEmployeeManagement.Data;
using PrisonEmployeeManagement.Models;

namespace PrisonEmployeeManagement.Services
{
    public class ConductService : IConductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ConductService> _logger;

        public ConductService(ApplicationDbContext context, ILogger<ConductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<EmployeeConduct> AddConductRecordAsync(EmployeeConduct conduct)
        {
            try
            {
                conduct.RecordedDate = DateTime.Now;
                conduct.CreatedAt = DateTime.Now;
                conduct.UpdatedAt = DateTime.Now;
                
                _context.EmployeeConducts.Add(conduct);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Added conduct record for employee {EmployeeId}", conduct.EmployeeId);
                return conduct;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding conduct record");
                throw;
            }
        }

        public async Task<EmployeeConduct> UpdateConductRecordAsync(EmployeeConduct conduct)
        {
            try
            {
                conduct.UpdatedAt = DateTime.Now;
                _context.Entry(conduct).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Updated conduct record {ConductId}", conduct.Id);
                return conduct;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating conduct record");
                throw;
            }
        }

        public async Task<bool> DeleteConductRecordAsync(int id)
        {
            try
            {
                var conduct = await _context.EmployeeConducts.FindAsync(id);
                if (conduct == null)
                    return false;
                    
                _context.EmployeeConducts.Remove(conduct);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Deleted conduct record {ConductId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting conduct record");
                throw;
            }
        }

        public async Task<EmployeeConduct?> GetConductRecordByIdAsync(int id)
        {
            return await _context.EmployeeConducts.FindAsync(id);
        }

        public async Task<IEnumerable<EmployeeConduct>> GetConductRecordsByEmployeeAsync(int employeeId)
        {
            return await _context.EmployeeConducts
                .Where(c => c.EmployeeId == employeeId)
                .OrderByDescending(c => c.IncidentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmployeeConduct>> GetConductByTypeAsync(int employeeId, string conductType)
        {
            return await _context.EmployeeConducts
                .Where(c => c.EmployeeId == employeeId && c.ConductType == conductType)
                .OrderByDescending(c => c.IncidentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmployeeConduct>> GetConductByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.EmployeeConducts
                .Where(c => c.IncidentDate >= startDate && c.IncidentDate <= endDate)
                .Include(c => c.Employee)
                .OrderByDescending(c => c.IncidentDate)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetConductStatisticsAsync(int employeeId)
        {
            var stats = new Dictionary<string, int>();
            
            stats["TotalGood"] = await _context.EmployeeConducts
                .CountAsync(c => c.EmployeeId == employeeId && c.ConductType == "Good");
                
            stats["TotalBad"] = await _context.EmployeeConducts
                .CountAsync(c => c.EmployeeId == employeeId && c.ConductType == "Bad");
                
            stats["PendingReview"] = await _context.EmployeeConducts
                .CountAsync(c => c.EmployeeId == employeeId && c.Status == "Under Review");
                
            stats["Resolved"] = await _context.EmployeeConducts
                .CountAsync(c => c.EmployeeId == employeeId && c.Status == "Resolved");
                
            return stats;
        }

        public async Task<bool> ExpungeConductRecordAsync(int id, string reason)
        {
            try
            {
                var conduct = await _context.EmployeeConducts.FindAsync(id);
                if (conduct == null)
                    return false;
                    
                conduct.Status = "Expunged";
                conduct.Remarks = $"Expunged: {reason}";
                conduct.UpdatedAt = DateTime.Now;
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Expunged conduct record {ConductId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error expunging conduct record");
                throw;
            }
        }
    }
}