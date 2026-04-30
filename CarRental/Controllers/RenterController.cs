using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace CarRental.Controllers
{
    
    public class RenterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RenterController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        // Show all cars
        
        public IActionResult ShowCars()
        {
            var vehicles = _context.Vehicles
                .Where(v => v.Availability == true)
                .ToList();

            return View(vehicles);
        }


        // Book car button

        [HttpPost]
        public IActionResult Book(int vehicleId)
        {
            return RedirectToAction("BookForm", new { vehicleId });
        }


        [Authorize]
        [HttpGet]
        public IActionResult BookForm(int vehicleId)
        {
            var vehicle = _context.Vehicles.FirstOrDefault(v => v.VehicleId == vehicleId);

            if (vehicle == null)
                return NotFound();

            return View(vehicle);
        }

        [HttpPost]
        public IActionResult CreateBooking(int vehicleId, DateTime startDate, DateTime endDate)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var renter = _context.Renters.FirstOrDefault(r => r.UserId == userId);

            if (renter == null)
                return Content("Renter not found");

            var vehicle = _context.Vehicles.FirstOrDefault(v => v.VehicleId == vehicleId);

            if (vehicle == null)
                return Content("Vehicle not found");

           

            //  Create booking
            var booking = new Booking
            {
                VehicleId = vehicleId,
                RenterId = renter.Id,
                StartDate = startDate,
                EndDate = endDate,
                Status = "Requested"
            };

            _context.Bookings.Add(booking);

            //  Make booked car unavailable
            vehicle.Availability = false;

            _context.SaveChanges();

            return RedirectToAction("Booking");
        }



    }
}
