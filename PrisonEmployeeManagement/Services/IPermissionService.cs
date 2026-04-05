using PrisonEmployeeManagement.Models;

namespace PrisonEmployeeManagement.Services
{
    public interface IPermissionService
    {
        Task<bool> HasPermission(int userId, string permissionName, string action);
        Task<List<string>> GetUserPermissions(int userId);
        Task<List<string>> GetUserRoles(int userId);
        Task<bool> AssignRole(int userId, int roleId, int assignedBy);
        Task<bool> RemoveRole(int userId, int roleId);
        Task<List<Role>> GetAllRoles();
        Task<List<Permission>> GetAllPermissions();
        Task<bool> CreateRole(Role role);
        Task<bool> UpdateRole(Role role);
        Task<bool> DeleteRole(int roleId);
        Task<bool> SetRolePermissions(int roleId, List<RolePermission> permissions);
        Task<UserDashboard> GetUserDashboard(int userId);
        Task UpdateLastLogin(int userId);
        Task<List<FileWorkflow>> GetUserInbox(int userId);
        Task<List<FileWorkflow>> GetUserSentFiles(int userId);
        Task<List<FileWorkflow>> GetUserRelatedFiles(int userId);
        Task<Dictionary<string, int>> GetUserDashboardStats(int userId);
    }
}