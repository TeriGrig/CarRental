namespace CarRental.Models
{
    public class Admin
    {
        public int AdminId { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
