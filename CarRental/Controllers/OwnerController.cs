using Azure.Core;
using CarRental.Data;
using CarRental.Models;
using CarRental.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
                Location = model.Location,
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
            .Where(v => v.OwnerID == owner.Id)
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

            _context.Vehicles.Remove(vehicle);
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
                Location = vehicle.Location,
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
            vehicle.Location = model.Location;
            vehicle.Comments = model.Comments;
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
    }
}
