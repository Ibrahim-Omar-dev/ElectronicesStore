using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using ElectronicsStore.Helper;
using ElectronicsStore.Data;
using Microsoft.AspNetCore.Identity;
using ElectronicsStore.Models;

namespace ElectronicsStore.Controllers
{
    [Authorize]
    public class CheckOutController : Controller
    {
        private string PaypalClientId { get; set; } = "";
        private string PaypalSecret { get; set; } = "";
        private string PaypalUrl { get; set; } = "";
        private readonly decimal shippingFee; // Fixed naming
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;

        public CheckOutController(IConfiguration configuration, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            PaypalClientId = configuration["Paypal:ClientId"] ?? "";
            PaypalSecret = configuration["Paypal:Secret"] ?? "";
            PaypalUrl = configuration["Paypal:Url"] ?? "";

            // Fixed configuration key and null handling
            if (decimal.TryParse(configuration["Cart:ShippingFee"], out decimal fee))
            {
                shippingFee = fee;
            }
            else
            {
                shippingFee = 0; // Default value if configuration is missing
            }

            this.context = context;
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
            var cartItems = CartHelper.GetCartList(Request, Response, context);
            decimal total = CartHelper.SubTotal(cartItems) + shippingFee;

            string deliveryAddress = TempData["DeliveryAddress"] as string ?? "";
            TempData.Keep();

            ViewBag.deliveryAddress = deliveryAddress;
            ViewBag.total = total;
            ViewBag.PaypalClientId = PaypalClientId;
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> CreateOrder()
        {
            try
            {
                var cartItems = CartHelper.GetCartList(Request, Response, context);

                // Validate cart items
                if (cartItems == null || !cartItems.Any())
                {
                    return new JsonResult(new { Id = "", Error = "Cart is empty" });
                }
                decimal totalAmount = CartHelper.SubTotal(cartItems) + shippingFee;

                // Build PayPal order request with compliance-friendly structure
                JsonObject createOrderRequest = new JsonObject
                {
                    ["intent"] = "CAPTURE",
                    ["purchase_units"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["reference_id"] = "default", // Add reference ID
                            ["amount"] = new JsonObject
                            {
                                ["currency_code"] = "USD",
                                ["value"] = totalAmount.ToString("F2")
                            },
                            ["description"] = "Electronics Store Purchase",
                            ["soft_descriptor"] = "ELEC_STORE"
                        }
                    },
                    ["application_context"] = new JsonObject
                    {
                        ["brand_name"] = "Electronics Store",
                        ["landing_page"] = "NO_PREFERENCE",
                        ["user_action"] = "PAY_NOW",
                        ["return_url"] = $"{Request.Scheme}://{Request.Host}/CheckOut/PaymentSuccess",
                        ["cancel_url"] = $"{Request.Scheme}://{Request.Host}/CheckOut/PaymentCancel"
                    }
                };

                string accessToken = await GetPaypalAccessToken();

                string url = PaypalUrl + "/v2/checkout/orders";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                    var request = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = new StringContent(createOrderRequest.ToJsonString(), Encoding.UTF8, "application/json")
                    };

                    var httpResponse = await client.SendAsync(request);
                    var responseContent = await httpResponse.Content.ReadAsStringAsync();

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        var jsonResponse = JsonDocument.Parse(responseContent);
                        var orderId = jsonResponse.RootElement.GetProperty("id").GetString();

                        return new JsonResult(new { Id = orderId });
                    }
                    else
                    {
                        return new JsonResult(new { Id = "", Error = responseContent });
                    }
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { Id = "", Error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> CaptureOrder([FromBody] JsonObject data)
        {
            try
            {
                var orderId = data?["orderId"]?.ToString();
                var deliveryAddress = data?["deliveryAddress"]?.ToString(); // Fixed casing

                if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(deliveryAddress))
                {
                    return new JsonResult(new { Status = "Failed", Message = "Invalid Order ID or Delivery Address" });
                }

                string accessToken = await GetPaypalAccessToken();
                string url = $"{PaypalUrl}/v2/checkout/orders/{orderId}/capture";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

                var httpResponse = await client.SendAsync(request);
                var responseContent = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                {
                    var jsonResponse = JsonDocument.Parse(responseContent);

                    // Save order to database
                    await SaveOrder(responseContent, deliveryAddress);

                    return new JsonResult(new { Status = "Success", Data = jsonResponse.RootElement });
                }
                else
                {
                    return new JsonResult(new { Status = "Failed", Error = responseContent });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { Status = "Failed", Error = ex.Message });
            }
        }

        private async Task SaveOrder(string paypalResponse, string deliveryAddress)
        {
            try
            {
                var cartItems = CartHelper.GetCartList(Request, Response, context);
                var appUser = await userManager.GetUserAsync(User);

                if (appUser == null || cartItems == null || !cartItems.Any())
                {
                    return;
                }

                var order = new Order
                {
                    ClientId = appUser.Id,
                    OrderItems = cartItems, // Fixed variable name
                    ShippingFee = (double)shippingFee, // Fixed property name
                    DeliveryAddress = deliveryAddress,
                    PaymentMethod = "paypal",
                    PaymentStatus = "Accepted",
                    PaymentDetails = paypalResponse,
                    OrderStatus = "Pending",
                    CreatedAt = DateTime.Now
                };

                context.Orders.Add(order);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the error (you should implement proper logging)
                throw new Exception($"Error saving order: {ex.Message}", ex);
            }
        }

        // Add these action methods for PayPal redirects
        public IActionResult PaymentSuccess()
        {
            return RedirectToAction("OrderSuccess", "Order");
        }

        public IActionResult PaymentCancel()
        {
            return RedirectToAction("Index", "Cart");
        }

        private async Task<string> GetPaypalAccessToken()
        {
            string accessToken = string.Empty;
            string url = $"{PaypalUrl}/v1/oauth2/token";

            using (var client = new HttpClient())
            {
                string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{PaypalClientId}:{PaypalSecret}"));

                client.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");

                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

                var httpResponse = await client.SendAsync(request);
                if (httpResponse.IsSuccessStatusCode)
                {
                    var responseContent = await httpResponse.Content.ReadAsStringAsync();
                    var jsonResponse = JsonDocument.Parse(responseContent);
                    accessToken = jsonResponse.RootElement.GetProperty("access_token").GetString() ?? "";
                }
                else
                {
                    throw new Exception($"Error retrieving access token: {httpResponse.StatusCode}");
                }
            }

            return accessToken;
        }
    }
}