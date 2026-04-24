namespace CarRental.Models
{
    public class Owner
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public ICollection<Vehicle> Vehicles { get; set; }
    }
}
