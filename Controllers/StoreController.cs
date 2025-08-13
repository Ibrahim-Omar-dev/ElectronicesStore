using ElectronicsStore.Data;
using ElectronicsStore.Models;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicsStore.Controllers
{
    public class StoreController : Controller
    {
        private readonly ApplicationDbContext context;
        private int PageSize = 8;

        public StoreController(ApplicationDbContext Context)
        {
            context = Context;
        }



        public IActionResult Index(int pageIndex, string? brand, string? category, string? sort, string? search)
        {
            IQueryable<Product> productsQuery = context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(brand))
                productsQuery = productsQuery.Where(p => p.Brand == brand);

            if (!string.IsNullOrEmpty(category))
                productsQuery = productsQuery.Where(p => p.Category == category);

            if (!string.IsNullOrEmpty(search))
                productsQuery = productsQuery.Where(p => p.Name.Contains(search));

            productsQuery = sort switch
            {
                "price_asc" => productsQuery.OrderBy(p => p.Price),
                "price_desc" => productsQuery.OrderByDescending(p => p.Price),
                _ => productsQuery.OrderByDescending(p => p.CreatedAt)
            };

            if (pageIndex < 1)
            {
                pageIndex = 1;
            }
            int count = productsQuery.Count();
            int totalPage = (int)Math.Ceiling((double)count / PageSize);
            productsQuery = productsQuery.Skip((pageIndex - 1) * PageSize).Take(PageSize);
            var product = productsQuery.ToList();

            ViewBag.pageIndex = pageIndex;
            ViewBag.totalPage = totalPage;
            var viewModel = new FilterVM
            {
                Products = product,
                Brand = brand,
                Category = category,
                Sort = sort,
                Search = search
            };

            return View(viewModel);
        }
        public IActionResult Details(int id)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
    }
}
