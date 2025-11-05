namespace MarinaMagazinOdezdiApp.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public int StockQuantity { get; set; }

        // Navigation property (optional, but good for related data)
        public Category Category { get; set; }
    }
}