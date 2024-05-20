using Models.Entities;
using UserService.Models;
using Microsoft.AspNetCore.Identity;

namespace UserService.Services
{
    public class RoleSeed
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly TelephoneDbContext _context;

        public RoleSeed(RoleManager<IdentityRole> roleManager, TelephoneDbContext context)
        {
            _roleManager = roleManager;
            _context = context;
        }

        public RoleSeed() { }

    public async Task SeedRolesAsync()
        {
            if (!await _roleManager.RoleExistsAsync(UserRoles.ADMIN))
            {
                // Create the Admin role if it doesn't exist
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.ADMIN));
            }

            if (!await _roleManager.RoleExistsAsync(UserRoles.SUBSCRIBER))
            {
                // Create the User role if it doesn't exist
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.SUBSCRIBER));
            }
        }
    }
}
