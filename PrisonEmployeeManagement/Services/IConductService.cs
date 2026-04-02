using PrisonEmployeeManagement.Models;

namespace PrisonEmployeeManagement.Services
{
    public interface IConductService
    {
        Task<EmployeeConduct> AddConductRecordAsync(EmployeeConduct conduct);
        Task<EmployeeConduct> UpdateConductRecordAsync(EmployeeConduct conduct);
        Task<bool> DeleteConductRecordAsync(int id);
        Task<EmployeeConduct?> GetConductRecordByIdAsync(int id);
        Task<IEnumerable<EmployeeConduct>> GetConductRecordsByEmployeeAsync(int employeeId);
        Task<IEnumerable<EmployeeConduct>> GetConductByTypeAsync(int employeeId, string conductType);
        Task<IEnumerable<EmployeeConduct>> GetConductByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetConductStatisticsAsync(int employeeId);
        Task<bool> ExpungeConductRecordAsync(int id, string reason);
    }
}