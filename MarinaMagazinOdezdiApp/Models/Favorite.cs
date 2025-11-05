namespace MarinaMagazinOdezdiApp.Models
{
    public class Favorite
    {
        public int FavoriteId { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Product Product { get; set; }
    }
}