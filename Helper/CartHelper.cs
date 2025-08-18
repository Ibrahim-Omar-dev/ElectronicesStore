using ElectronicsStore.Data;
using ElectronicsStore.Models;
using System.Text;
using System.Text.Json;

namespace ElectronicsStore.Helper
{
    public class CartHelper
    {
        public static Dictionary<int, int> GetCartDictionary(HttpRequest httpRequest, HttpResponse httpResponse)
        {
            var cartValue = httpRequest.Cookies["shopping_cart"] ?? "";

            try
            {
                var cart = Encoding.UTF8.GetString(Convert.FromBase64String(cartValue));
                var cartDictionary = JsonSerializer.Deserialize<Dictionary<int, int>>(cart);

                if (cartDictionary != null)
                {
                    return cartDictionary;
                }
            }
            catch (Exception)
            {
            }
            if (cartValue.Length > 0)
            {
                httpResponse.Cookies.Delete("shopping_cart");
            }
            return new Dictionary<int, int>();
        }
        public static int GetSize(HttpRequest httpRequest, HttpResponse httpResponse)
        {
            int size = 0;
            var cartDictionary = GetCartDictionary(httpRequest, httpResponse);
            foreach (var cart in cartDictionary)
            {
                size += cart.Value;
            }
            return size;
        }

        public static List<OrderItem> GetCartList(HttpRequest request, HttpResponse response, ApplicationDbContext context)
        {
            var cartIems = new List<OrderItem>();
            var cartDictionary = GetCartDictionary(request, response);
            foreach (var cart in cartDictionary)
            {
                var key = cart.Key;
                var value = cart.Value;
                var product = context.Products.FirstOrDefault(p => p.Id == key);
                var item = new OrderItem
                {
                    Price = product.Price,
                    Quentity = value,
                    Product = product
                };
                cartIems.Add(item);
            }
            return cartIems;
        }
        public static decimal SubTotal(List<OrderItem> cartList)
        {
            decimal total = 0;
            foreach (var item in cartList)
            {
                total += item.Price * item.Quentity;
            }
            return total;
        }
    }
}
