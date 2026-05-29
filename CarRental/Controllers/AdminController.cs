using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
        public async Task<IActionResult> AdminHome()
        {
            return View();
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
    }
}