using ElectronicsStore.Data;
using ElectronicsStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStore.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("/Admin/Orders/{action=index}/{id?}")]
    public class AdminOrderController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly int pageSize = 5;
        public AdminOrderController(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IActionResult Index(int pageIndex)
        {
            IQueryable<Order> query = context.Orders.Include(o => o.Client).Include(o => o.OrderItems);

            if (pageIndex < 1)
            {
                pageIndex = 1;
            }
            int totalOrders = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
            query = query.OrderByDescending(o => o.CreatedAt)
                         .Skip((pageIndex - 1) * pageSize)
                         .Take(pageSize);

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageIndex;
            var orders = query.ToList();
            return View(orders);
        }
        public IActionResult Details(int id)
        {
            var order = context.Orders.Include(o => o.Client)
                .Include(o => o.OrderItems).ThenInclude(i => i.Product).FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.OrderCount = context.Orders.Where(o => o.ClientId == order.ClientId).Count();

            return View(order);
        }
        public IActionResult Edit(int id, string? PaymentStatus, string? OrderStatus)
        {
            var order = context.Orders.FirstOrDefault(o => o.Id == id);
            if(order == null)
            {
                return RedirectToAction("index");
            }
            if(PaymentStatus != null)
            {
                order.PaymentStatus = PaymentStatus;
            }
            if(OrderStatus != null)
            {
                order.OrderStatus = OrderStatus;
            }
            context.SaveChanges();
            return RedirectToAction("details", new { id });
        }
    }
}
