using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gateway.Controllers
{
    [ApiController]
    [Route("diag")]
    public class DiagController : ControllerBase
    {
        [HttpGet("cookies")]
        public IActionResult GetCookies()
        {
            var cookies = Request.Cookies.ToDictionary(c => c.Key, c => c.Value);

            return Ok(new
            {
                hasCookies = cookies.Any(),
                cookieKeys = cookies.Keys,
                hasAccessToken = Request.Cookies.ContainsKey("accessToken"),
                hasRefreshToken = Request.Cookies.ContainsKey("refreshToken")
            });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetMe()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var company = User.FindFirst("Company")?.Value;
            var tokenExpiry = User.FindFirst("exp")?.Value;

            DateTimeOffset? expiry = null;
            if (long.TryParse(tokenExpiry, out var exp))
                expiry = DateTimeOffset.FromUnixTimeSeconds(exp);

            return Ok(new
            {
                isAuthenticated = User.Identity?.IsAuthenticated,
                userId,
                role,
                company,
                tokenExpiresAt = expiry?.ToString("o")
            });
        }

        [HttpGet("headers")]
        [Authorize]
        public IActionResult GetHeaders()
        {
            return Ok(new
            {
                xUserId = Request.Headers["X-User-Id"].ToString(),
                xUserRole = Request.Headers["X-User-Role"].ToString(),
                xUserCompany = Request.Headers["X-User-Company"].ToString(),
                hasAuthHeader = Request.Headers.ContainsKey("Authorization")
            });
        }
    }
}
