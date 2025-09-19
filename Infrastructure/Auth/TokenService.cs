using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Auth;

public interface ITokenService {
    Task<string> GenerateTokenAsync(IdentityUser user);
}

public class TokenService : ITokenService {
    private readonly JwtSettings _settings;
    private readonly UserManager<IdentityUser> _userManager;

    public TokenService(IOptions<JwtSettings> options, UserManager<IdentityUser> userManager) {
        _settings = options.Value;
        _userManager = userManager;
    }

    public async Task<string> GenerateTokenAsync(IdentityUser user) {
        var userRoles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim> {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

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