using Microsoft.AspNetCore.Identity;
using PrisonEmployeeManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace PrisonEmployeeManagement.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var context = services.GetRequiredService<ApplicationDbContext>();

            // Create roles
            string[] roles = { "System Admin", "HR Admin", "Manager", "Supervisor", "Employee" };
            
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create admin user if not exists
            var adminEmail = "admin@prison.gov";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
                // Check if employee exists
                var adminEmployee = await context.Employees.FirstOrDefaultAsync(e => e.EmployeeNumber == "ADMIN001");
                
                if (adminEmployee == null)
                {
                    adminEmployee = new Employee
                    {
                        EmployeeNumber = "ADMIN001",
                        FirstName = "System",
                        LastName = "Administrator",
                        Email = adminEmail,
                        PhoneNumber = "(555) 000-0000",
                        Position = "System Administrator",
                        Department = "IT",
                        HireDate = DateTime.Now,
                        DateOfBirth = new DateTime(1980, 1, 1),
                        Gender = "Male",
                        EmploymentStatus = "Active",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    context.Employees.Add(adminEmployee);
                    await context.SaveChangesAsync();
                }

                var user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    EmployeeId = adminEmployee.Id,
                    Department = "IT",
                    Position = "System Administrator",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(user, "Admin@123");
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "System Admin");
                    await userManager.AddToRoleAsync(user, "HR Admin");
                }
            }

            // Create demo employee user
            var demoEmail = "john.smith@prison.gov";
            var demoUser = await userManager.FindByEmailAsync(demoEmail);
            
            if (demoUser == null)
            {
                var demoEmployee = await context.Employees.FirstOrDefaultAsync(e => e.EmployeeNumber == "PEN001");
                
                if (demoEmployee != null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = demoEmail,
                        Email = demoEmail,
                        FirstName = demoEmployee.FirstName,
                        LastName = demoEmployee.LastName,
                        EmployeeId = demoEmployee.Id,
                        Department = demoEmployee.Department,
                        Position = demoEmployee.Position,
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    };

                    var result = await userManager.CreateAsync(user, "Employee@123");
                    
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Employee");
                    }
                }
            }
        }
    }
}