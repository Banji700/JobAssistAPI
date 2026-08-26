using System.ComponentModel.DataAnnotations;

namespace JobApplica.LoginRegister
{
    public class RegisterDto
    {
        [Required]
        public string Role { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
