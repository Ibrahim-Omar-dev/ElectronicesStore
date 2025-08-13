namespace ElectronicsStore.Models
{
    public class FilterVM
    {
        public List<Product> Products { get; set; }
        public string? Category { get; set; }
        public string? Brand { get; set; }
        public string? Sort { get; set; }
        public string? Search { get; set; }
    }
}
