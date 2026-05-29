using CarRental.Data;
using CarRental.Models;
using CarRental.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using System.Globalization;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using System.Threading.Tasks;




namespace CarRental.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public UserController(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewProfile()
        {
            var userId = _userManager.GetUserId(User);

            var reviews = await _context.Reviews
            .Include(r => r.Commenter)
            .Where(r => r.RecipientId == userId && !r.IsDeleted)
            .ToListAsync();
            ViewBag.Reviews = reviews;

            if (User.IsInRole("Renter"))
            {
                //ρεντερ και οουνερ μαζι 
                var renter = _context.Renters
                    .Include(r => r.User)  //ινκλιουντ για να φέρουμε τα στοιχεία του χρήστη
                    .FirstOrDefault(r => r.UserId == userId && !r.User.IsDeleted);

                if (renter == null) return NotFound();

                return View(renter);
            }
            else if (User.IsInRole("Owner"))
            {
                var owner = _context.Owners
                    .Include(o => o.User)
                    .FirstOrDefault(o => o.UserId == userId && !o.User.IsDeleted);

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

            var emailExists = await _userManager.Users.AnyAsync(u => u.Email == Email && u.Id != userId);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "This email is already in use.");

                if (User.IsInRole("Renter"))
                {
                    var renter = _context.Renters
                        .Include(r => r.User)
                        .FirstOrDefault(r => r.UserId == userId);

                    return View(renter);
                }

                var owner = _context.Owners
                    .Include(o => o.User)
                    .FirstOrDefault(o => o.UserId == userId);

                return View(owner);
            }

            var phoneExists = await _userManager.Users.AnyAsync(u => u.PhoneNumber == PhoneNumber && u.Id != userId);

            if (phoneExists)
            {
                ModelState.AddModelError("PhoneNumber", "This phone number is already in use.");

                if (User.IsInRole("Renter"))
                {
                    var renter = _context.Renters
                        .Include(r => r.User)
                        .FirstOrDefault(r => r.UserId == userId);

                    return View(renter);
                }

                var owner = _context.Owners
                    .Include(o => o.User)
                    .FirstOrDefault(o => o.UserId == userId);

                return View(owner);
            }

            await _userManager.SetEmailAsync(user, Email);
            await _userManager.SetUserNameAsync(user, Email);

            var result = await _userManager.UpdateAsync(user);


            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);

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
        public IActionResult DeleteUser()
        {
            var userId = _userManager.GetUserId(User);

            var user = _context.Users
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteUserConfirmed()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return NotFound();

            user.IsDeleted = true;
            user.Email = null;
            user.NormalizedEmail = null;
            user.UserName = $"deleted_{user.Id}";
            user.NormalizedUserName = user.UserName.ToUpper();
            user.PhoneNumber = null;

            if (User.IsInRole("Owner"))
            {
                var owner = _context.Owners
                    .Include(o => o.User)
                    .FirstOrDefault(o => o.UserId == userId);
                if (owner == null) return NotFound();

                var vehicles = _context.Vehicles.Where(v => v.OwnerID == owner.Id && !v.IsDeleted);

                foreach (var v in vehicles)
                {
                    var result = new OwnerController(_context).DeleteCar(v.VehicleId);
                }
            }

            await _userManager.UpdateAsync(user);

            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");

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

            var reviews = await _context.Reviews
             .Include(r => r.Commenter)
             .Where(r => r.RecipientId == userId && !r.IsDeleted)
             .ToListAsync();
            ViewBag.Reviews = reviews;

            var renter = await _context.Renters.Include(r => r.User).FirstOrDefaultAsync(r => r.UserId == userId && !r.User.IsDeleted);
            if (renter != null) return View("OpenUsersProfile", renter);

            var owner = await _context.Owners.Include(o => o.User).FirstOrDefaultAsync(o => o.UserId == userId && !o.User.IsDeleted);
            if (owner != null) return View("OpenUsersProfile", owner);

            return NotFound();


        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyBookings()
        {
            var userId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin"))
            {
                if (User.IsInRole("Owner"))
                {
                    var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

                    var bookings = _context.Bookings
                    .Where(b => b.Vehicle.OwnerID == owner.Id)
                    .Include(b => b.Vehicle)
                    .Include(b => b.Renter)
                     .ThenInclude(r => r.User)
                    .ToList();
                    return View(bookings);
                }
                if (User.IsInRole("Renter"))
                {
                    var renter = await _context.Renters.FirstOrDefaultAsync(r => r.UserId == userId);
                    var bookings = _context.Bookings
                    .Where(b => b.RenterId == renter.Id)
                    .Include(b => b.Vehicle)
                    .Include(b => b.Vehicle.Owner)
                        .ThenInclude(r => r.User)
                    .ToList();
                    return View(bookings);
                }
            }
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult CancelBooking(int bookingId)
        {
            var booking = _context.Bookings
                .Include(b => b.Vehicle)
                .FirstOrDefault(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return Content("Booking not found");
            }
            booking.Status = "Cancelled";
            booking.Vehicle.Availability = true;
            _context.SaveChanges();
            return RedirectToAction("MyBookings");

        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ReportUser(string recipientId)
        {
         
            var reporterId = User.FindFirstValue(ClaimTypes.NameIdentifier);

          

            if (string.IsNullOrEmpty(recipientId))
            {
                return BadRequest("Invalid user ID.");
            }

            
            var newReport = new Report
            {
                ReporterId = reporterId,
                ReportRecipientId = recipientId,
                DateTime = DateTime.Now,
                Seen = false
            };

           
            _context.Reports.Add(newReport);
            await _context.SaveChangesAsync();

            TempData["Success"] = "User has been successfully reported to administration.";
            return RedirectToAction("OpenUsersProfile", new { userId = recipientId });
        }
    }
}