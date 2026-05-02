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
    }

}