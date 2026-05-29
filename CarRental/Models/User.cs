using Microsoft.AspNetCore.Identity;

namespace CarRental.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsSuspended { get; set; } = false;
        public DateOnly SuspensionEnd { get; set; }
    }
}
