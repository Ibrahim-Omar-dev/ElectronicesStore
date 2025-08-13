using ElectronicsStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;

namespace ElectronicsStore.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("Admin/[controller]/{action=Index}/{id?}")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }
        public IActionResult Index()
        {
            var users = userManager.Users.OrderByDescending(x => x.Id).ToList();
            return View(users);
        }
        public async Task<IActionResult> Details(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index", "user");
            }

            var appUser = await userManager.FindByIdAsync(id);
            if (appUser == null)
            {
                return RedirectToAction("Index", "user");
            }

            ViewBag.Roles = await userManager.GetRolesAsync(appUser);


            var availableRole = roleManager.Roles.ToList();
            var items = new List<SelectListItem>();
            foreach (var role in availableRole)
            {
                items.Add(new SelectListItem
                {
                    Text = role.NormalizedName,
                    Value = role.Name,
                    Selected = await userManager.IsInRoleAsync(appUser, role.Name)
                });
            }
            ViewBag.SelectItems = items;
            return View(appUser);
        }
        [HttpPost]
        public async Task<IActionResult> EditRole(string id, string newRole)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(newRole))
            {
                TempData["ErrorMessage"] = "Invalid request.";
                return RedirectToAction("Index", "User");
            }

            var appUser = await userManager.FindByIdAsync(id);
            if (appUser == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index", "User");
            }

            var roleExists = await roleManager.RoleExistsAsync(newRole);
            if (!roleExists)
            {
                TempData["ErrorMessage"] = $"Role '{newRole}' does not exist.";
                return RedirectToAction("Details", new { id });
            }

            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser != null && currentUser.Id == appUser.Id)
            {
                TempData["ErrorMessage"] = "You cannot change your own role.";
                return RedirectToAction("Details", new { id });
            }

            var userRoles = await userManager.GetRolesAsync(appUser);

            // Remove old roles if any
            if (userRoles.Any())
            {
                var removeResult = await userManager.RemoveFromRolesAsync(appUser, userRoles);
                if (!removeResult.Succeeded)
                {
                    TempData["ErrorMessage"] = "Failed to remove old roles.";
                    return RedirectToAction("Details", new { id });
                }
            }

            // Add new role
            var addResult = await userManager.AddToRoleAsync(appUser, newRole);
            if (!addResult.Succeeded)
            {
                TempData["ErrorMessage"] = "Failed to assign new role.";
                return RedirectToAction("Details", new { id });
            }

            TempData["SuccessMessage"] = "Role updated successfully.";
            return RedirectToAction("Details", new { id });
        }

        public async Task<IActionResult> DeleteUser(string? id)
        {
            if (id == null)
            {
                return RedirectToAction("details");
            }
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return RedirectToAction("details");
            }
            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser != null && currentUser.Id == user.Id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction("Details", new { id });
            }
            var result = await userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete user.";
            }
            return RedirectToAction("index");
        }
    }
}
