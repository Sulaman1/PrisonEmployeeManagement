using Microsoft.EntityFrameworkCore;
using PrisonEmployeeManagement.Data;
using PrisonEmployeeManagement.Models;

namespace PrisonEmployeeManagement.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(ApplicationDbContext context, ILogger<PermissionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> HasPermission(int userId, string permissionName, string action)
        {
            try
            {
                var userRoles = await _context.UserRoles
                    .Include(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .Where(ur => ur.UserId == userId && ur.IsActive)
                    .ToListAsync();

                foreach (var userRole in userRoles)
                {
                    if (userRole.Role?.RolePermissions != null)
                    {
                        var permission = userRole.Role.RolePermissions
                            .FirstOrDefault(rp => rp.Permission != null && rp.Permission.PermissionName == permissionName);

                        if (permission != null)
                        {
                            return action switch
                            {
                                "View" => permission.CanView,
                                "Create" => permission.CanCreate,
                                "Edit" => permission.CanEdit,
                                "Delete" => permission.CanDelete,
                                "Approve" => permission.CanApprove,
                                _ => false
                            };
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission for user {UserId}", userId);
                return false;
            }
        }

        public async Task<List<string>> GetUserPermissions(int userId)
        {
            var permissions = new List<string>();
            try
            {
                var userRoles = await _context.UserRoles
                    .Include(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .Where(ur => ur.UserId == userId && ur.IsActive)
                    .ToListAsync();

                foreach (var userRole in userRoles)
                {
                    if (userRole.Role?.RolePermissions != null)
                    {
                        permissions.AddRange(userRole.Role.RolePermissions
                            .Where(rp => rp.Permission != null)
                            .Select(rp => rp.Permission!.PermissionName));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user permissions for user {UserId}", userId);
            }
            return permissions.Distinct().ToList();
        }

        public async Task<List<string>> GetUserRoles(int userId)
        {
            try
            {
                var userRoles = await _context.UserRoles
                    .Include(ur => ur.Role)
                    .Where(ur => ur.UserId == userId && ur.IsActive)
                    .ToListAsync();

                return userRoles.Select(ur => ur.Role?.RoleName ?? string.Empty)
                    .Where(r => !string.IsNullOrEmpty(r))
                    .ToList()!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles for user {UserId}", userId);
                return new List<string>();
            }
        }

        public async Task<bool> AssignRole(int userId, int roleId, int assignedBy)
        {
            try
            {
                // Check if already assigned
                var existing = await _context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.IsActive);
                
                if (existing != null)
                {
                    return true; // Already assigned
                }

                var userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    AssignedDate = DateTime.Now,
                    AssignedBy = assignedBy,
                    IsActive = true
                };
                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role to user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> RemoveRole(int userId, int roleId)
        {
            try
            {
                var userRole = await _context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
                if (userRole != null)
                {
                    _context.UserRoles.Remove(userRole);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role from user {UserId}", userId);
                return false;
            }
        }

        public async Task<List<Role>> GetAllRoles()
        {
            try
            {
                return await _context.Roles
                    .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all roles");
                return new List<Role>();
            }
        }

        public async Task<List<Permission>> GetAllPermissions()
        {
            try
            {
                return await _context.Permissions.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all permissions");
                return new List<Permission>();
            }
        }

        public async Task<bool> CreateRole(Role role)
        {
            try
            {
                role.CreatedAt = DateTime.Now;
                role.UpdatedAt = DateTime.Now;
                _context.Roles.Add(role);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
                return false;
            }
        }

        public async Task<bool> UpdateRole(Role role)
        {
            try
            {
                role.UpdatedAt = DateTime.Now;
                _context.Entry(role).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role");
                return false;
            }
        }

        public async Task<bool> DeleteRole(int roleId)
        {
            try
            {
                var role = await _context.Roles.FindAsync(roleId);
                if (role != null)
                {
                    _context.Roles.Remove(role);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role");
                return false;
            }
        }

        public async Task<bool> SetRolePermissions(int roleId, List<RolePermission> permissions)
        {
            try
            {
                // Remove existing permissions
                var existingPermissions = _context.RolePermissions.Where(rp => rp.RoleId == roleId);
                _context.RolePermissions.RemoveRange(existingPermissions);

                // Add new permissions
                foreach (var permission in permissions)
                {
                    permission.RoleId = roleId;
                    _context.RolePermissions.Add(permission);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting role permissions");
                return false;
            }
        }

        public async Task<UserDashboard> GetUserDashboard(int userId)
        {
            try
            {
                var dashboard = await _context.UserDashboards
                    .FirstOrDefaultAsync(ud => ud.UserId == userId);

                if (dashboard == null)
                {
                    dashboard = new UserDashboard
                    {
                        UserId = userId,
                        DashboardType = "Employee",
                        LoginCount = 0
                    };
                    _context.UserDashboards.Add(dashboard);
                    await _context.SaveChangesAsync();
                }

                return dashboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user dashboard for user {UserId}", userId);
                return new UserDashboard { UserId = userId, DashboardType = "Employee", LoginCount = 0 };
            }
        }

        public async Task UpdateLastLogin(int userId)
        {
            try
            {
                var dashboard = await GetUserDashboard(userId);
                dashboard.LastLogin = DateTime.Now;
                dashboard.LoginCount++;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last login for user {UserId}", userId);
            }
        }

        public async Task<List<FileWorkflow>> GetUserInbox(int userId)
        {
            try
            {
                return await _context.FileWorkflows
                    .Include(w => w.File)
                    .Include(w => w.FromEmployee)
                    .Include(w => w.ToEmployee)
                    .Where(w => w.ToEmployeeId == userId && w.Status != "Completed")
                    .OrderByDescending(w => w.IsUrgent)
                    .ThenByDescending(w => w.SentDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user inbox for user {UserId}", userId);
                return new List<FileWorkflow>();
            }
        }

        public async Task<List<FileWorkflow>> GetUserSentFiles(int userId)
        {
            try
            {
                return await _context.FileWorkflows
                    .Include(w => w.File)
                    .Include(w => w.FromEmployee)
                    .Include(w => w.ToEmployee)
                    .Where(w => w.FromEmployeeId == userId)
                    .OrderByDescending(w => w.SentDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sent files for user {UserId}", userId);
                return new List<FileWorkflow>();
            }
        }

        public async Task<List<FileWorkflow>> GetUserRelatedFiles(int userId)
        {
            try
            {
                return await _context.FileWorkflows
                    .Include(w => w.File)
                    .Include(w => w.FromEmployee)
                    .Include(w => w.ToEmployee)
                    .Where(w => w.FromEmployeeId == userId || w.ToEmployeeId == userId)
                    .OrderByDescending(w => w.SentDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting related files for user {UserId}", userId);
                return new List<FileWorkflow>();
            }
        }

        public async Task<Dictionary<string, int>> GetUserDashboardStats(int userId)
        {
            var stats = new Dictionary<string, int>();
            try
            {
                stats["Inbox"] = await _context.FileWorkflows.CountAsync(w => w.ToEmployeeId == userId && w.Status != "Completed");
                stats["Sent"] = await _context.FileWorkflows.CountAsync(w => w.FromEmployeeId == userId);
                stats["Pending"] = await _context.FileWorkflows.CountAsync(w => w.ToEmployeeId == userId && w.Status == "Pending");
                stats["InProgress"] = await _context.FileWorkflows.CountAsync(w => w.ToEmployeeId == userId && w.Status == "In Progress");
                stats["Completed"] = await _context.FileWorkflows.CountAsync(w => w.ToEmployeeId == userId && w.Status == "Completed");
                stats["Urgent"] = await _context.FileWorkflows.CountAsync(w => w.ToEmployeeId == userId && w.IsUrgent && w.Status != "Completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats for user {UserId}", userId);
            }
            return stats;
        }
    }
}