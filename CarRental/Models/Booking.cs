namespace CarRental.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public int RenterId { get; set; }
        public Renter Renter { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
    }
}
