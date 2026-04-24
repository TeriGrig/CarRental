namespace CarRental.Models
{
    public class Vehicle
    {
        public int VehicleId { get; set; }
        public int OwnerID { get; set; }
        public Owner Owner { get; set; }
        public string Image { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Cubic { get; set; }
        public int Year { get; set; }
        public int PricePerDay { get; set; }
        public bool Availability { get; set; }
        public string Location { get; set; }
        public string Comments { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }
}
