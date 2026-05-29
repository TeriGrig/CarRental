using CarRental.Models;
using Microsoft.AspNetCore.Identity;

namespace CarRental.Middleware
{
    public class SuspensionMiddleware
    {
        private readonly RequestDelegate _next;

        public SuspensionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, UserManager<User> userManager)
        {
            var path = context.Request.Path.Value?.ToLower();

            if (path != null &&
                (path.Contains("/home/suspended") ||
                 path.Contains("/identity") ||
                 path.Contains("/account") ||
                 path.Contains("/css") ||
                 path.Contains("/js") ||
                 path.Contains("/images")))
            {
                await _next(context);
                return;
            }

            if (context.User.Identity?.IsAuthenticated == true)
            {   
                var user = await userManager.GetUserAsync(context.User);

                if (user != null &&
                    user.IsSuspended &&
                    user.SuspensionEnd > DateOnly.FromDateTime(DateTime.Now))
                {
                    context.Response.Redirect("/Home/Suspended");
                    return;
                }
                else if(user != null && user.IsSuspended && user.SuspensionEnd <= DateOnly.FromDateTime(DateTime.Now))
                {
                    user.IsSuspended = false;
                    await userManager.UpdateAsync(user);
                }
            }

            await _next(context);
        }
    }
}
