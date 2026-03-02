namespace CoffeeHavenDB.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        public decimal DiscountPercentage { get; set; }

        public int CategoryId { get; set; }
    }
}