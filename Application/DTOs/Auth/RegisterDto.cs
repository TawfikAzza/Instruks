namespace Application.DTOs.Auth;

public class RegisterDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }  // Should be "Doctor" or "Nurse"
}