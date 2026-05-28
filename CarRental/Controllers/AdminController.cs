using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;


        [HttpGet]
        public async Task<IActionResult> AdminHome()
        {
            return View();
        }

        public async Task<IActionResult> AdminDeleteUser()
        {
            var user = User.
            return RedirectToAction("Index", "Home");
        }
    }

}
