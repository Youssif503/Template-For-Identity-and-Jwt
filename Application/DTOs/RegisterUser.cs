using System.ComponentModel.DataAnnotations;

namespace JWT_Train.Application.DTOs
{
    public class RegisterUser
    {
        [Required,StringLength(256)]
        public string UserName { get; set; }
        [Required,StringLength(100)]
        public string FirstName { get; set; }
        [Required,StringLength(100)]
        public string LastName { get; set; }
        [Required]
        [EmailAddress(ErrorMessage =" Invalid email address , Must Be Contain @.")]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
