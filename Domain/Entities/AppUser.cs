using Microsoft.AspNetCore.Identity;
namespace JWT_Train.Domain.Entities
{
    public class AppUser:IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
