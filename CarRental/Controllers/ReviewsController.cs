using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ReviewsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddReview(string recipientId, string comment, int rating)
        {
            try
            {
                var commenterId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(commenterId) || string.IsNullOrEmpty(recipientId))
                    return Json(new { success = false, message = "Invalid Data" });

                var newReview = new Review
                {
                    CommenterId = commenterId,
                    RecipientId = recipientId,
                    Rating = rating,
                    Comment = comment ?? "",
                    IsDeleted = false
                };

                _context.Reviews.Add(newReview);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
