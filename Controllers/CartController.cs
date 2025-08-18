using ElectronicsStore.Data;
using ElectronicsStore.Helper;
using ElectronicsStore.Models;
using ElectronicsStore.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicsStore.Controllers
{
    public class CartController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration configuration;
        private readonly ApplicationDbContext context;
        private readonly string shippingFee;
        public CartController(UserManager<ApplicationUser> userManager, IConfiguration configuration, ApplicationDbContext context)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.context = context;
            shippingFee = configuration["Cart:ShippingFee"] ?? "0";
        }
        public IActionResult Index()
        {
            List<OrderItem> cartDictionary = Helper.CartHelper.GetCartList(Request, Response, context);
            var subTotal = CartHelper.SubTotal(cartDictionary);

            ViewBag.shippingFee = decimal.Parse(shippingFee);
            ViewBag.SubTotal = subTotal;
            ViewBag.Total = subTotal + decimal.Parse(shippingFee);
            ViewBag.CartItems = cartDictionary;


            return View();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(CheckDto checkDto)
        {
            if (!ModelState.IsValid)
            {
                // re-fetch cart in case of validation errors
                var cartItems = Helper.CartHelper.GetCartList(Request, Response, context);
                ViewBag.CartItems = cartItems;
                ViewBag.SubTotal = CartHelper.SubTotal(cartItems);
                ViewBag.ShippingFee = decimal.Parse(shippingFee);
                ViewBag.Total = ViewBag.SubTotal + ViewBag.ShippingFee;
                return View(checkDto);
            }

            var cartDictionary = Helper.CartHelper.GetCartList(Request, Response, context);
            var subTotal = CartHelper.SubTotal(cartDictionary);

            if (cartDictionary.Count == 0)
            {
                ViewBag.ErrorMessage = "Your Cart Was Empty";
                return View(checkDto);
            }

            ViewBag.CartItems = cartDictionary;
            ViewBag.SubTotal = subTotal;
            ViewBag.ShippingFee = decimal.Parse(shippingFee);
            ViewBag.Total = subTotal + ViewBag.ShippingFee;

            // Use TempData instead of ViewData for redirect
            TempData["DeliveryAddress"] = checkDto.Delviery;
            TempData["PaymentMethod"] = checkDto.PaymentMethod;

            return RedirectToAction("Confirm");
        }


        // GET: /Cart/Confirm
        [Authorize]
        [HttpGet]
        public IActionResult Confirm()
        {
            var cartItems = CartHelper.GetCartList(Request, Response, context);
            var subTotal = CartHelper.SubTotal(cartItems);
            decimal total = subTotal + decimal.Parse(shippingFee);
            int size = CartHelper.GetSize(Request, Response);

            string deliveryAddress = TempData.Peek("DeliveryAddress") as string ?? "";
            string paymentMethod = TempData.Peek("PaymentMethod") as string ?? "";

            if (size == 0 || deliveryAddress.Length == 0 || paymentMethod.Length == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Quentity = size;
            ViewBag.Total = total;
            ViewBag.DeliveryAddress = deliveryAddress;
            ViewBag.PaymentMethod = paymentMethod;

            return View();
        }

        // POST: /Cart/Confirm
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(string deliveryAddress, string paymentMethod)
        {
            var cartItems = CartHelper.GetCartList(Request, Response, context);

            if (cartItems.Count == 0 || string.IsNullOrEmpty(deliveryAddress) || string.IsNullOrEmpty(paymentMethod))
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = new Order
            {
                ClientId = user.Id,
                DeliveryAddress = deliveryAddress,
                PaymentMethod = paymentMethod,
                OrderItems = cartItems,
                CreatedAt = DateTime.Now,
                PaymentStatus = "Pending",
                PaymentDetails = "",
                OrderStatus = "Pending",
                TotalPrice = cartItems.Sum(item => item.Price * item.Quentity) + decimal.Parse(shippingFee),
            };

            context.Add(order);
            context.SaveChanges();

            Response.Cookies.Delete("shopping_cart");

            TempData["SuccessMessage"] = "Your order has been placed successfully!";
            return RedirectToAction("Confirm"); // reload GET to show success
        }

    }
}
