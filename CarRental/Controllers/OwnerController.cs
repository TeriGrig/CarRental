using Azure.Core;
using CarRental.Data;
using CarRental.Models;
using CarRental.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.Controllers
{
    public class OwnerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OwnerController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        [HttpGet]
        public IActionResult AddCar()
        {
            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> AddCar([Bind(Prefix = "")] AddVehicleViewModel model)
        {
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
                Comments = model.Comments,
                Availability = true,
                Image = imagePath?? "/images/car-placeholder.png",
                OwnerID = 1
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }


    }
}
