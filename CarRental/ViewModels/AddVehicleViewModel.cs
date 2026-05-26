using Microsoft.AspNetCore.Http;
using Microsoft.Build.Framework;
using System.Diagnostics.CodeAnalysis;

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
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
        public string? Comments { get; set; }

        public bool IsAvailable { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}

