using Azure.Core;
using CarRental.Data;
using CarRental.Models;
using CarRental.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;

namespace CarRental.Controllers
{
    public class OwnerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OwnerController(ApplicationDbContext context)
        {
            _context = context;
        }

        //[Authorize]
        [Authorize(Roles = "Owner")]
        [HttpGet]
        public IActionResult AddCar()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> AddCar([Bind(Prefix = "")] AddVehicleViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var owner = _context.Owners.FirstOrDefault(r => r.UserId == userId);

            if (owner == null)
                return Content("Owner not found");

            if (model.Latitude == 0 && model.Longitude == 0)
            {
                return Content("Please select a location on the map");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .SelectMany(x => x.Value.Errors)
                    .Select(e => e.ErrorMessage);

                return Content("Model invalid:\n" + string.Join("\n", errors));
            }

            string imagePath = null;


            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                imagePath = "/images/" + fileName;
            }


            var vehicle = new Vehicle
            {
                Make = model.Make,
                Model = model.Model,
                Cubic = model.Cubic ?? 0,
                Year = model.Year ?? 0,
                PricePerDay = model.PricePerDay ?? 0,
                //Latitude = (model.Latitude >= -90 && model.Latitude <= 90) ? model.Latitude : 0,
                //Longitude = (model.Longitude >= -180 && model.Longitude <= 180) ? model.Longitude : 0,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                Comments = model.Comments ?? "",
                Availability = model.IsAvailable,
                Image = imagePath ?? "/images/car-placeholder.png",
                OwnerID = owner.Id
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }





        //[Authorize]
        [Authorize(Roles = "Owner")]
        [HttpGet]
        public IActionResult ViewMyCar()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var owner = _context.Owners.FirstOrDefault(r => r.UserId == userId);

            if (owner == null)

            { return Content("Owner not found"); }

            var vehicles = _context.Vehicles
            .Where(v => v.OwnerID == owner.Id && !v.IsDeleted)
            .ToList();

            return View(vehicles);
        }

        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteCar(int vehicleId)
        {
            var vehicle = _context.Vehicles.FirstOrDefault(v => v.VehicleId == vehicleId);
            if (vehicle == null)
            {
                return NotFound();
            }

            if (!vehicle.Availability)
            {
                ModelState.AddModelError("", "Car is already booked!");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var owner = _context.Owners.FirstOrDefault(o => o.UserId == userId);

                var vehicles = _context.Vehicles
                    .Where(v => v.OwnerID == owner.Id && !v.IsDeleted)
                    .ToList();

                return View("ViewMyCar", vehicles);
            }

            var archivedBookings = _context.Bookings
                .Where(b =>
                    b.VehicleId == vehicle.VehicleId &&
                    !b.Vehicle.IsDeleted &&
                    (b.Status == "Cancelled"
                    || b.Status == "Rejected"
                    || b.EndDate < DateTime.Now))
                .ToList();

            if (archivedBookings.Count() > 0)
            {
                vehicle.IsDeleted = true;
                _context.Vehicles.Update(vehicle);
            }
            else
            {
                _context.Vehicles.Remove(vehicle);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ViewMyCar");
        }


        [HttpGet]
        [Authorize(Roles = "Owner")]
        public IActionResult EditMyCar(int vehicleId)
        {
            var vehicle = _context.Vehicles.FirstOrDefault(v => v.VehicleId == vehicleId);
            if (vehicle == null)
            {
                return NotFound();
            }


            var model = new AddVehicleViewModel
            {
                Make = vehicle.Make,
                Model = vehicle.Model,
                Cubic = vehicle.Cubic,
                Year = vehicle.Year,
                PricePerDay = vehicle.PricePerDay,
                Latitude = vehicle.Latitude,
                Longitude = vehicle.Longitude,
                Comments = vehicle.Comments,
                ImageFile = null,
                IsAvailable = vehicle.Availability
            };
            return View(model);
            
        }
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> EditMyCar(int vehicleId, [Bind(Prefix = "")] AddVehicleViewModel model)
        {
            var vehicle = _context.Vehicles.FirstOrDefault(v => v.VehicleId == vehicleId);
            if (vehicle == null)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .SelectMany(x => x.Value.Errors)
                    .Select(e => e.ErrorMessage);
                return Content("Model invalid:\n" + string.Join("\n", errors));
            }
            vehicle.Make = model.Make;
            vehicle.Model = model.Model;
            vehicle.Cubic = model.Cubic ?? 0;
            vehicle.Year = model.Year ?? 0;
            vehicle.PricePerDay = model.PricePerDay ?? 0;
            vehicle.Comments = model.Comments ?? "";
            vehicle.Latitude = model.Latitude;
            vehicle.Longitude = model.Longitude;
            vehicle.Availability = model.IsAvailable;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                Directory.CreateDirectory(uploadsFolder);
                string fileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }
                vehicle.Image = "/images/" + fileName;
            }
            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();
            return RedirectToAction("ViewMyCar");
        }




        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> UpdateBookingStatus(int bookingId, string Status)
        {
            var booking = await _context.Bookings
                .Include(b => b.Vehicle)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return NotFound();

            booking.Status = Status;

            if (Status == "Accepted")
            {
                booking.Vehicle.Availability = false;
            }
            else if (Status == "Rejected")
            {
                booking.Vehicle.Availability = true;
            }

            _context.Update(booking);

            await _context.SaveChangesAsync();


            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }


            return RedirectToAction("MyBookings", "User");
        }

    }
}
