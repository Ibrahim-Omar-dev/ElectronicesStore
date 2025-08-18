using ElectronicsStore.Data;
using ElectronicsStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace ElectronicsStore.Controllers
{
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly int PageSize = 5;
        public ClientController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index(int pageIndex)
        {
            var appUser = await userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return RedirectToAction("Index", "Home");
            }
            IQueryable<Order> query = context.Orders.Include(c => c.OrderItems).ThenInclude(i => i.Product)
                .OrderDescending().Where(o => o.ClientId == appUser.Id);

            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }
            int TotalItems = query.Count();
            int TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
            query = query.Skip((pageIndex - 1) * PageSize).Take(PageSize);


            ViewBag.TotalPages = TotalPages;
            ViewBag.CurrentPage = pageIndex;
            var orders = query.ToList();

            return View(query);
        }
        public async Task<IActionResult> Details(int id)
        {
            var appUser = await userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var order = await context.Orders.Include(c => c.OrderItems).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.ClientId == appUser.Id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }
    }
}
