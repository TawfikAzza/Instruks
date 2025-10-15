using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    /// <summary>
    /// Credentials used to request a JWT.
    /// </summary>
    public class LoginDto
    {
        /// <summary>Email address of the user.</summary>
        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        /// <summary>User's password.</summary>
        [Required, StringLength(128, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;
    }
}