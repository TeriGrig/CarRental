using CarRental.Data;
using CarRental.Models;
using CarRental.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;
using Stripe.Checkout;

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
            ViewBag.StartDate = "";
            ViewBag.StartTime = "";
            ViewBag.EndDate = "";
            ViewBag.EndTime = "";

            var vehicles = _context.Vehicles
                .Where(v => v.Availability && !v.IsDeleted)
                .ToList();

            return View(vehicles);
        }

        [HttpGet]
        public IActionResult SearchCars(SearchViewModel model)
        {
            var start = model.StartDate.Date + model.StartTime.TimeOfDay;
            var end = model.EndDate.Date + model.EndTime.TimeOfDay;

            if (start >= end)
            {
                ModelState.AddModelError("", "End date must be after start date");
                return View();
            }

            var availableVehicles = _context.Vehicles
                .Where(v => !v.IsDeleted && v.Availability)
                .Where(v => !v.Bookings.Any(b =>
                    (b.Status == "Accepted" || b.Status == "Requested")
                    &&
                    start < b.EndDate
                    &&
                    end > b.StartDate
                ))
                .ToList();

            ViewBag.StartDate = model.StartDate.ToString("yyyy-MM-dd");
            ViewBag.StartTime = model.StartTime.ToString(@"HH\:mm");

            ViewBag.EndDate = model.EndDate.ToString("yyyy-MM-dd");
            ViewBag.EndTime = model.EndTime.ToString(@"HH\:mm");

            // full datetime for booking
            ViewBag.FullStart = start.ToString("yyyy-MM-ddTHH:mm:ss");
            ViewBag.FullEnd = end.ToString("yyyy-MM-ddTHH:mm:ss");

            return View("ShowCars", availableVehicles);
        }


        // Book car button

        [HttpPost]
        public IActionResult Book(int vehicleId, DateTime startDate, DateTime endDate)
        {
            if (startDate >= endDate || startDate == DateTime.MinValue || endDate == DateTime.MinValue)
            {
                TempData["BookingError"] = "Please enter valid booking dates.";

                return RedirectToAction("ShowCars");
            }
            
            return RedirectToAction(
                "BookForm",
                new
                {
                    vehicleId,
                    startDate,
                    endDate
                });
        }


        [Authorize(Roles = "Renter")]
        [HttpGet]
        public IActionResult BookForm(int vehicleId, DateTime startDate, DateTime endDate)
        {
            var vehicle = _context.Vehicles
                .Include(v => v.Owner)
                .ThenInclude(o => o.User)
                .FirstOrDefault(v => v.VehicleId == vehicleId);

            if (vehicle == null)
                return NotFound();

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

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
                Status = "Requested",
                
            };

            _context.Bookings.Add(booking);

           
            //vehicle.Availability = false;
            _context.SaveChanges();
            TempData["Success"] = "Request created!";

            return RedirectToAction("ShowCars");
       
        }

        [HttpGet]
        [Route("Renter/PayBooking")]
        public IActionResult PayBooking(int bookingId)
        {
            var booking = _context.Bookings
                .Include(b => b.Vehicle)
                .FirstOrDefault(b => b.BookingId == bookingId);

            if (booking == null)
                return NotFound();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> {"card"},

                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Quantity = 1,

                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "eur",

                            UnitAmount =  booking.Vehicle.PricePerDay * 100,

                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = booking.Vehicle.Make + " " + booking.Vehicle.Model
                            }
                        }
                    }
                },

                Mode = "payment",

                SuccessUrl =  "https://localhost:7146/Renter/PaymentSuccess?bookingId=" + bookingId,

                CancelUrl = "https://localhost:7146/User/MyBookings"
            };

            var service = new SessionService();

            Session session = service.Create(options);

            return Redirect(session.Url);
        }

        [HttpGet]
        [Authorize(Roles = "Renter")]
        public async Task<IActionResult> PaymentSuccess(int bookingId)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return NotFound();

            booking.IsPaid = true;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyBookings", "User");
        }
    }
}
