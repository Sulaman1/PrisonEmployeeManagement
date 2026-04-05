using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrisonEmployeeManagement.Models;
using PrisonEmployeeManagement.Services;
using PrisonEmployeeManagement.Data;
using Microsoft.AspNetCore.Authorization;

namespace PrisonEmployeeManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IPermissionService permissionService,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _permissionService = permissionService;
            _logger = logger;
        }

        // GET: Account/Login
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "UserDashboard");
            }
            
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (ModelState.IsValid)
            {
                // Find user by email or username
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    user = await _userManager.FindByNameAsync(model.Email);
                }

                if (user != null)
                {
                    // Check if user is active
                    if (!user.IsActive)
                    {
                        ModelState.AddModelError(string.Empty, "Your account has been deactivated. Please contact administrator.");
                        return View(model);
                    }

                    // Sign in
                    var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                    
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User logged in: {Email}", model.Email);
                        
                        // Update last login
                        user.LastLoginAt = DateTime.Now;
                        await _userManager.UpdateAsync(user);
                        
                        // Update dashboard last login
                        await _permissionService.UpdateLastLogin(user.EmployeeId ?? 0);
                        
                        // Get user roles for session
                        var roles = await _userManager.GetRolesAsync(user);
                        
                        // Redirect based on role
                        if (string.IsNullOrEmpty(returnUrl))
                        {
                            return RedirectToAction("Index", "UserDashboard", new { userId = user.EmployeeId });
                        }
                        
                        return LocalRedirect(returnUrl);
                    }
                }
                
                ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your email and password.");
            }
            
            return View(model);
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "UserDashboard");
            }
            
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if employee exists in Employees table
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.EmployeeNumber == model.EmployeeNumber);
                
                if (employee == null)
                {
                    ModelState.AddModelError("EmployeeNumber", "Employee number not found. Please contact HR.");
                    return View(model);
                }

                // Check if user already exists for this employee
                var existingUser = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.EmployeeId == employee.Id);
                
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "An account already exists for this employee.");
                    return View(model);
                }

                // Create new user
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    EmployeeId = employee.Id,
                    Department = employee.Department,
                    Position = employee.Position,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("New user registered: {Email}", model.Email);
                    
                    // Assign default role "Employee"
                    await _userManager.AddToRoleAsync(user, "Employee");
                    
                    // Create user dashboard
                    var dashboard = new UserDashboard
                    {
                        UserId = employee.Id,
                        DashboardType = "Employee",
                        LoginCount = 0
                    };
                    _context.UserDashboards.Add(dashboard);
                    await _context.SaveChangesAsync();

                    // Sign in the user
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    
                    return RedirectToAction("Index", "UserDashboard", new { userId = employee.Id });
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            
            return View(model);
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out");
            return RedirectToAction("Login", "Account");
        }

        // GET: Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: Account/Profile
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(user.EmployeeId);
            var roles = await _userManager.GetRolesAsync(user);

            var model = new ProfileViewModel
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Department = employee?.Department,
                Position = employee?.Position,
                EmployeeNumber = employee?.EmployeeNumber,
                Roles = roles.ToList(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };

            return View(model);
        }

        // GET: Account/ChangePassword
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: Account/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // GET: Account/ResetPassword
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }
            
            var model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }
    }
}