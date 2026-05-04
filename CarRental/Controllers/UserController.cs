using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
using CarRental.ViewModels;
using System.Globalization;

namespace CarRental.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public UserController(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewProfile()
        {
            var userId = _userManager.GetUserId(User);

            if (User.IsInRole("Renter"))
            {
                //ρεντερ και οουνερ μαζι 
                var renter = _context.Renters
                    .Include(r => r.User)  //ινκλιουντ για να φέρουμε τα στοιχεία του χρήστη
                    .FirstOrDefault(r => r.UserId == userId);

                if (renter == null) return NotFound();

                return View(renter);
            }
            else if (User.IsInRole("Owner"))
            {
                var owner = _context.Owners
                    .Include(o => o.User)
                    .FirstOrDefault(o => o.UserId == userId);

                if (owner == null) return NotFound();

                return View(owner);
            }

            return NotFound();
        }


        [HttpGet]
        [Authorize]
        public IActionResult EditProfile()
        {
            var userId = _userManager.GetUserId(User);
            if (User.IsInRole("Renter"))
            {
                var renter = _context.Renters
                    .Include(r => r.User)
                    .FirstOrDefault(r => r.UserId == userId);

                if (renter == null) return NotFound();
                return View(renter);
            }

            if (User.IsInRole("Owner"))
            {
                var owner = _context.Owners
                    .Include(o => o.User)
                    .FirstOrDefault(o => o.UserId == userId);
                if (owner == null) return NotFound();
                return View(owner);
            }

            return NotFound();


        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditProfile(string FirstName, string LastName, string PhoneNumber, int? BirthYear, string Email, int? LicenceYear)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return NotFound();

            user.FirstName = FirstName;
            user.LastName = LastName;
            user.PhoneNumber = PhoneNumber;
            user.Email = Email;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                if (User.IsInRole("Renter"))
                {
                    var renter = _context.Renters.FirstOrDefault(r => r.UserId == userId);
                    if (renter != null)
                    {
                        renter.BirthYear = BirthYear ?? 0;
                        renter.LicenceYear = LicenceYear ?? 0;
                        _context.Update(renter);
                        await _context.SaveChangesAsync();
                    }
                }
                return RedirectToAction(nameof(ViewProfile));
            }

            return View(user);
        }


        [HttpGet]
        public async Task<IActionResult> BookingNotifications()

        {
            var userId = _userManager.GetUserId(User);
            if (User.IsInRole("Owner"))
            {
                var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);
                if (owner == null) return NotFound();

                var bookings = await _context.Bookings
                .Include(b => b.Vehicle)
                .Include(b => b.Renter.User)
                .Where(b => b.Vehicle.OwnerID == owner.Id && b.Status == "Requested")
                .Select(b => new
                {
                    id = b.BookingId,
                    message = "Booking for " + b.Vehicle.Make + " from " + b.Renter.User.FirstName,
                    profileUrl = "/User/OpenUsersProfile?userId=" + b.Renter.UserId

                })
                  .ToListAsync();

                return Json(bookings);

            }
            if (User.IsInRole("Renter"))
            {
                var renter = await _context.Renters.FirstOrDefaultAsync(r => r.UserId == userId);
                if (renter == null) return NotFound();
                var bookings = await _context.Bookings
                .Include(b => b.Vehicle)
                .Include(b => b.Vehicle.Owner.User)
                .Where(b => b.RenterId == renter.Id)
                .Select(b => new
                {
                    id = b.BookingId,
                    message = b.Status == "Requested"
                        ? "Your booking for " + b.Vehicle.Make + " is pending approval from " + b.Vehicle.Owner.User.FirstName
                        : "Your booking for " + b.Vehicle.Make + " has been " + b.Status.ToLower() + " by " + b.Vehicle.Owner.User.FirstName, 
                        profileUrl = "/User/OpenUsersProfile?userId=" + b.Vehicle.Owner.UserId

                })
                  .ToListAsync();
                return Json(bookings);


            }

            return NotFound();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> OpenUsersProfile(string userId)
        {

            if (string.IsNullOrEmpty(userId)) return NotFound();

            var renter = await _context.Renters.Include(r => r.User).FirstOrDefaultAsync(r => r.UserId == userId);
            if (renter != null) return View("OpenUsersProfile", renter);

            var owner = await _context.Owners.Include(o => o.User).FirstOrDefaultAsync(o => o.UserId == userId);
            if (owner != null) return View("OpenUsersProfile", owner);

            return NotFound();


        }

        }
}