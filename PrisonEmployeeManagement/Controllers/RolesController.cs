using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PrisonEmployeeManagement.Data;
using PrisonEmployeeManagement.Models;
using PrisonEmployeeManagement.Services;

namespace PrisonEmployeeManagement.Controllers
{
    public class RolesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(
            ApplicationDbContext context,
            IPermissionService permissionService,
            ILogger<RolesController> logger)
        {
            _context = context;
            _permissionService = permissionService;
            _logger = logger;
        }

        // GET: Roles
        public async Task<IActionResult> Index()
        {
            var roles = await _context.Roles
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .ToListAsync();
            return View(roles);
        }

        // GET: Roles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Role role)
        {
            if (ModelState.IsValid)
            {
                await _permissionService.CreateRole(role);
                TempData["SuccessMessage"] = "Role created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        // GET: Roles/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        // POST: Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Role role)
        {
            if (id != role.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _permissionService.UpdateRole(role);
                TempData["SuccessMessage"] = "Role updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        // GET: Roles/Permissions/5
        public async Task<IActionResult> Permissions(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var allPermissions = await _context.Permissions.ToListAsync();
            var rolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == id)
                .ToDictionaryAsync(rp => rp.PermissionId, rp => rp);

            ViewBag.Role = role;
            ViewBag.Permissions = allPermissions;
            ViewBag.RolePermissions = rolePermissions;

            return View();
        }

        // POST: Roles/Permissions
        [HttpPost]
        public async Task<IActionResult> Permissions(int roleId, List<int> viewPermissions, List<int> createPermissions, 
            List<int> editPermissions, List<int> deletePermissions, List<int> approvePermissions)
        {
            var permissions = new List<RolePermission>();
            var allPermissions = await _context.Permissions.ToListAsync();

            foreach (var perm in allPermissions)
            {
                permissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = perm.Id,
                    CanView = viewPermissions?.Contains(perm.Id) ?? false,
                    CanCreate = createPermissions?.Contains(perm.Id) ?? false,
                    CanEdit = editPermissions?.Contains(perm.Id) ?? false,
                    CanDelete = deletePermissions?.Contains(perm.Id) ?? false,
                    CanApprove = approvePermissions?.Contains(perm.Id) ?? false
                });
            }

            await _permissionService.SetRolePermissions(roleId, permissions);
            TempData["SuccessMessage"] = "Permissions updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Roles/Assign
        public async Task<IActionResult> Assign()
        {
            ViewBag.Users = new SelectList(await _context.Employees.ToListAsync(), "Id", "FullName");
            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "Id", "RoleName");
            return View();
        }

        // POST: Roles/Assign
        [HttpPost]
        public async Task<IActionResult> Assign(int userId, int roleId)
        {
            var currentUser = await _context.Employees.FirstOrDefaultAsync();
            await _permissionService.AssignRole(userId, roleId, currentUser?.Id ?? 1);
            TempData["SuccessMessage"] = "Role assigned successfully!";
            return RedirectToAction("Index", "Employees");
        }
    }
}