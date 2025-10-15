using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Auth
{
    /// <summary>
    /// Abstraction for issuing JSON Web Tokens (JWT) for authenticated users.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a signed JWT for the specified <see cref="IdentityUser"/>.
        /// </summary>
        /// <param name="user">The authenticated user to issue a token for.</param>
        /// <returns>
        /// A JWT serialized as a compact string that includes user identity and role claims.
        /// </returns>
        /// <remarks>
        /// The token’s issuer, audience, signing key, and lifetime are derived from <see cref="JwtSettings"/>.
        /// Role claims are added using <see cref="ClaimTypes.Role"/> for use with ASP.NET Core authorization.
        /// </remarks>
        Task<string> GenerateTokenAsync(IdentityUser user);
    }

    /// <summary>
    /// Default implementation of <see cref="ITokenService"/> that creates HMAC-signed JWTs
    /// using settings provided via <see cref="JwtSettings"/>.
    /// </summary>
    /// <remarks>
    /// The generated token includes:
    /// <list type="bullet">
    ///   <item><description><c>sub</c> (subject): the user ID.</description></item>
    ///   <item><description><c>email</c>: the user email (if present).</description></item>
    ///   <item><description><c>jti</c>: a unique token identifier.</description></item>
    ///   <item><description>One or more <see cref="ClaimTypes.Role"/> claims for the user's roles.</description></item>
    /// </list>
    /// The token is signed with <see cref="SecurityAlgorithms.HmacSha256"/> using <see cref="JwtSettings.SecretKey"/>.
    /// </remarks>
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _settings;
        private readonly UserManager<IdentityUser> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenService"/> class.
        /// </summary>
        /// <param name="options">An options wrapper containing <see cref="JwtSettings"/>.</param>
        /// <param name="userManager">ASP.NET Core Identity user manager for role retrieval.</param>
        public TokenService(IOptions<JwtSettings> options, UserManager<IdentityUser> userManager)
        {
            _settings = options.Value;
            _userManager = userManager;
        }

        /// <summary>
        /// Generates a signed JWT for the specified user that includes identity and role claims.
        /// </summary>
        /// <param name="user">The authenticated user.</param>
        /// <returns>A compact JWT string suitable for use in the <c>Authorization: Bearer &lt;token&gt;</c> header.</returns>
        /// <remarks>
        /// Token lifetime is set to <see cref="JwtSettings.ExpiryMinutes"/> minutes from the current UTC time.
        /// The token uses the <see cref="JwtSettings.Issuer"/> and <see cref="JwtSettings.Audience"/> configured
        /// for the application. The consumer (API) must validate issuer, audience, lifetime, and signature.
        /// </remarks>
        public async Task<string> GenerateTokenAsync(IdentityUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                // Subject (user identifier)
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),

                // Email (optional but helpful for audit/UI)
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),

                // Unique token identifier
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add role claims for policy/role-based authorization
            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
