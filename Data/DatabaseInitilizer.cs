using ElectronicsStore.Models;
using Microsoft.AspNetCore.Identity;

namespace ElectronicsStore.Data
{
    public class DatabaseInitilizer
    {
        public async Task SeedData(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));

            if (roleManager == null)
                throw new ArgumentNullException(nameof(roleManager));

            // Check if the role already exists
            if (!await roleManager.RoleExistsAsync("admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("admin"));
            }
            if (!await roleManager.RoleExistsAsync("seller"))
            {
                await roleManager.CreateAsync(new IdentityRole("seller"));
            }
            if (!await roleManager.RoleExistsAsync("client"))
            {
                await roleManager.CreateAsync(new IdentityRole("client"));
            }
            //check if we have at least admin user
            var adminUser = await userManager.GetUsersInRoleAsync("admin");
            if (adminUser.Any())
            {
                return; // We already have an admin user
            }
            // Create a new admin user
            var admin = new ApplicationUser
            {
                FirstName = "admin",
                LastName = "admin",
                Email = "admin@gmail.com",
                UserName = "admin",
                EmailConfirmed = true,
                CreateAt = DateTime.Now
            };
            var password = "Hema-2003";
            var result = await userManager.CreateAsync(admin, password);
            if (result.Succeeded)
            {
                // Assign the admin role to the new user
                await userManager.AddToRoleAsync(admin, "admin");
            }
            else
            {
                throw new Exception("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
