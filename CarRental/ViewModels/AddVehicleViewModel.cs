using Microsoft.AspNetCore.Http;
using Microsoft.Build.Framework;

namespace CarRental.ViewModels
{
    public class AddVehicleViewModel
    {
        [Required]
        public string Make { get; set; }
        [Required]
        public string Model { get; set; }
        [Required]
        public int? Cubic { get; set; }
        [Required]
        public int? Year { get; set; }
        [Required]
        public int? PricePerDay { get; set; }
        [Required]
        public string Location { get; set; }
        public string Comments { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}

