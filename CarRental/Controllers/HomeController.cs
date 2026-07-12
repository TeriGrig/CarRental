using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CarRental.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            //var vehicles = _context.Vehicles.ToList();
            var vehicles = _context.Vehicles
            .Include(v => v.Owner)
            .ThenInclude(o => o.User)
            .Where(v =>
                v.Availability &&
                !v.IsDeleted &&
                !v.Owner.User.IsSuspended)
            .ToList();
            return View(vehicles);
           
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Suspended()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                ViewBag.SuspensionEnd = user?.SuspensionEnd;
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
