using E_Commerce.API_V9_.Utilities.DbInitialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace E_Commerce.API_V9_.Utilities
{
    public class DbInitilizer : IDbInitilizer
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DbInitilizer(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager , ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }
        public async Task Initialize()
        {
            if (_context.Database.GetPendingMigrations().Any())
            {
                _context.Database.Migrate();
            }
            if (_roleManager.Roles.Any())
            {
               await _roleManager.CreateAsync(new IdentityRole(SD.ROLE_SUPER_ADMIN));
                await _roleManager.CreateAsync(new IdentityRole(SD.ROLE_ADMIN));
                await _roleManager.CreateAsync(new IdentityRole(SD.ROLE_EMPLOYEE));
                await _roleManager.CreateAsync(new IdentityRole(SD.ROLE_CUSTOMER));

                await _userManager.CreateAsync(new ()
                {
                    FName= "Super",
                    LName= "Admin",
                    Email= "superadmin@example.com",
                    EmailConfirmed = true,
                    UserName = "superadmin",
                  
                },"SuperAdmin123*");
                await _userManager.CreateAsync(new()
                {
                    FName = "Admin",
                    LName = "",
                    Email = "admin@example.com",
                    EmailConfirmed = true,
                    UserName = "admin",

                }, "Admin123*");
                await _userManager.CreateAsync(new()
                {
                    FName = "Employee",
                    LName = "1",
                    Email = "employee1@example.com",
                    EmailConfirmed = true,
                    UserName = "employee1",

                }, "Employee123*");
                await _userManager.CreateAsync(new()
                {
                    FName = "Employee",
                    LName = "2",
                    Email = "employee2@example.com",
                    EmailConfirmed = true,
                    UserName = "employee2",

                }, "Employee123*");
                var user = await _userManager.FindByNameAsync("SuperAdmin");
                var user2 = await _userManager.FindByNameAsync("Admin");
                var user3 = await _userManager.FindByNameAsync("Employee1");
                var user4 = await _userManager.FindByNameAsync("Employee2");
                if (user != null && user2 != null && user3 != null && user4 != null) 
                {
                await _userManager.AddToRoleAsync(user, SD.ROLE_SUPER_ADMIN);
                    await _userManager.AddToRoleAsync(user2, SD.ROLE_ADMIN);
                    await _userManager.AddToRoleAsync(user3, SD.ROLE_EMPLOYEE);
                    await _userManager.AddToRoleAsync(user4, SD.ROLE_EMPLOYEE);
                }
            }
        }
    }
}
