using PrisonEmployeeManagement.Models;

namespace PrisonEmployeeManagement.Services
{
    public interface IEmployeeService
    {
        // Basic CRUD
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<Employee?> GetEmployeeByIdAsync(int id);
        Task<Employee> CreateEmployeeAsync(Employee employee);
        Task<Employee> UpdateEmployeeAsync(Employee employee);
        Task<bool> DeleteEmployeeAsync(int id);
        Task<bool> EmployeeExistsAsync(int id);
        
        // Search and Filter
        Task<IEnumerable<Employee>> SearchEmployeesAsync(string searchTerm);
        Task<IEnumerable<Employee>> GetEmployeesByDepartmentAsync(string department);
        Task<IEnumerable<Employee>> GetEmployeesByStatusAsync(string status);
        Task<IEnumerable<Employee>> GetEmployeesByRankAsync(string rank);
        
        // Distinct Values
        Task<IEnumerable<string>> GetDistinctDepartmentsAsync();
        Task<IEnumerable<string>> GetDistinctPositionsAsync();
        Task<IEnumerable<string>> GetDistinctRanksAsync();
        
        // Statistics
        Task<Dictionary<string, int>> GetEmployeeStatisticsAsync();
        
        // Enhanced Methods
        Task<Employee?> GetEmployeeWithFullHistoryAsync(int id);
        Task<IEnumerable<EmployeeConduct>> GetEmployeeConductHistoryAsync(int employeeId);
        Task<IEnumerable<EmployeePosting>> GetEmployeePostingHistoryAsync(int employeeId);
        Task<IEnumerable<EmployeeTraining>> GetEmployeeTrainingHistoryAsync(int employeeId);
        Task<IEnumerable<EmployeeAward>> GetEmployeeAwardsAsync(int employeeId);
        Task<IEnumerable<EmployeeLeave>> GetEmployeeLeaveHistoryAsync(int employeeId);
        
        // Performance Metrics
        Task<double> GetEmployeeAttendanceRateAsync(int employeeId, int year);
        Task<int> GetEmployeeDisciplinaryCountAsync(int employeeId);
        Task<int> GetEmployeeAwardCountAsync(int employeeId);
    }
}