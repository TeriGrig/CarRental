using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(
            UserManager<User> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult CreateAdmin()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> LiftSuspension(string UserId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == UserId);

            if (user != null)
            {
              
                user.IsSuspended = false;
                user.SuspensionEnd = default; 

                _context.Update(user);
                await _context.SaveChangesAsync();
            }

      
            return RedirectToAction("OpenUsersProfile", "User", new { userId = UserId });
        }


        [HttpGet]
        public async Task<IActionResult> AdminHome()
        {
            ViewBag.TotalUsersCount = await _context.Users.CountAsync();
            ViewBag.TotalVehiclesCount = await _context.Vehicles.CountAsync();

      
            ViewBag.Owners = await _context.Owners
                .Include(o => o.User)
                .Where(o => !o.User.IsDeleted)
                .ToListAsync();

            ViewBag.Renters = await _context.Renters
                .Include(r => r.User)
                .Where(r => !r.User.IsDeleted)
                .ToListAsync();

       
            ViewBag.AllVehicles = await _context.Vehicles
                .Include(v => v.Owner)
                .ThenInclude(o => o.User)
                .ToListAsync();

            ViewBag.AllBookings = await _context.Bookings
                .Include(b => b.Vehicle)
                .Include(b => b.Renter)
                .ThenInclude(r => r.User)
                .ToListAsync();

            var ownerUserIds = await _context.Owners.Select(o => o.User.Id).ToListAsync();
            var renterUserIds = await _context.Renters.Select(r => r.User.Id).ToListAsync();

            ViewBag.Admins = await _context.Users
            .Where(u => !u.IsDeleted && !ownerUserIds.Contains(u.Id) && !renterUserIds.Contains(u.Id))
            .ToListAsync();
            return View("AdminHome");
        }

        public async Task<IActionResult> SuspendUser(string UserId, DateOnly date)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == UserId);

            if (user != null)
            {
                user.IsSuspended = true;
                user.SuspensionEnd = date;
                _context.Update(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Home");
        }




        [HttpPost]
        public async Task<IActionResult> CreateAdmin(
            string firstName,
            string lastName,
            string email,
            string password)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null && !existingUser.IsDeleted)
            {
                ModelState.AddModelError("", "Email already exists.");
                return View();
            }

            var user = new User
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View();
            }

            await _userManager.AddToRoleAsync(user, "Admin");

            var admin = new Admin
            {
                UserId = user.Id
            };

            _context.Admins.Add(admin);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Admin created successfully.";

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> ShowReports()
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportRecipient)
                .OrderByDescending(r => r.DateTime)
                .ToListAsync();

            return View(reports);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> InvestigateAndMarkAsSeen(int reportId, string recipientId)
        {
            
            var report = await _context.Reports.FirstOrDefaultAsync(r => r.ReportId == reportId);
            if (report != null && !report.Seen)
            {
                report.Seen = true;
                _context.Update(report);
                await _context.SaveChangesAsync();
            }

            
            return RedirectToAction("OpenUsersProfile", "User", new { userId = recipientId });
        }


        [HttpGet]
        public async Task<IActionResult> ShowSuspendedUsers()
        {
            var suspendedUsers = await _context.Users
                .Where(u => u.IsSuspended && !u.IsDeleted)
                .ToListAsync();
            return View(suspendedUsers);
        }


   
    }
}