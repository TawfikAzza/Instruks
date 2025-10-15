using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    /// <summary>
    /// Registration payload. Role must be either <c>Doctor</c> or <c>Nurse</c>.
    /// </summary>
    public class RegisterDto
    {
        /// <summary>Email address for the new account.</summary>
        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        /// <summary>Password for the new account.</summary>
        [Required, StringLength(128, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        /// <summary>Role name (Doctor or Nurse).</summary>
        [Required]
        [RegularExpression("^(Doctor|Nurse)$", ErrorMessage = "Role must be 'Doctor' or 'Nurse'.")]
        public string Role { get; set; } = "Nurse";
    }
}