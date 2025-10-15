using Application.DTOs.Auth;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

/// <summary>
/// Authentication endpoints for registering users and issuing JWTs.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ITokenService _tokenService;

    /// <summary>Creates a new <see cref="AuthController"/>.</summary>
    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Register a new user account. Role must be <c>Doctor</c> or <c>Nurse</c>.
    /// </summary>
    /// <param name="dto">Registration payload.</param>
    /// <remarks>
    /// This endpoint is anonymous. Consider restricting in production or adding email verification.
    /// </remarks>
    /// <response code="200">User registered successfully.</response>
    /// <response code="400">Validation failed or identity errors.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthLimiter")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto) {
        // Basic server-side guard (FluentValidation + Data Annotations also run).
        if (dto.Role != "Doctor" && dto.Role != "Nurse")
            return BadRequest("Invalid role. Allowed values: 'Doctor' or 'Nurse'.");

        var user = new IdentityUser
        {
            UserName = dto.Email,
            Email = dto.Email
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var roleResult = await _userManager.AddToRoleAsync(user, dto.Role);
        if (!roleResult.Succeeded)
            return BadRequest(roleResult.Errors);

        return Ok("User registered successfully.");
    }

    /// <summary>
    /// Authenticate a user and return a JWT access token.
    /// </summary>
    /// <param name="dto">Login credentials.</param>
    /// <response code="200">Authenticated; returns the token.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Invalid credentials.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthLimiter")] // ← rate-limit brute force attempts
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return Unauthorized("Invalid credentials.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
            return Unauthorized("Invalid credentials.");

        var token = await _tokenService.GenerateTokenAsync(user);
        return Ok(new { token });
    }
}
