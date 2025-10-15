using System.Linq;
using System.Security.Claims;
using Application.Interfaces.Security;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Security
{
    /// <summary>
    /// ASP.NET Core-backed implementation of <see cref="ICurrentUser"/> using <see cref="IHttpContextAccessor"/>.
    /// Lives in Infrastructure to avoid web dependencies in Application.
    /// </summary>
    public sealed class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _http;

        public CurrentUser(IHttpContextAccessor http) => _http = http;

        /// <inheritdoc />
        public string? UserId =>
            _http.HttpContext?.User?.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        /// <inheritdoc />
        public bool IsDoctor =>
            _http.HttpContext?.User?.IsInRole("Doctor") == true;

        /// <inheritdoc />
        public bool IsNurse =>
            _http.HttpContext?.User?.IsInRole("Nurse") == true;
    }
}