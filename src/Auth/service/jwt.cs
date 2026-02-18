using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedLibraries.model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Auth.service
{
    interface IJwt
    {
        public Task<string> GenerateAccessTokenAsync(Claims claims);
        public string GenerateRefreshTokenAsync();
        public void ValidateToken(string token);
    }

    public interface IClaims
    {
        Guid userId { get; set; }
        Guid companyId { get; set; }
        CompanyUserRole role { get; set; }
    }

    public class Claims : IClaims
    {
        public Guid userId { get; set; }
        public Guid companyId { get; set; }
        public CompanyUserRole role { get; set; }
    }

    public static class ClaimsService
    {
        public static List<Claim> GetClaims(Claims claims)
        {
            return new List<Claim>
            {
                new Claim(ClaimTypes.Name, claims.userId.ToString()),
                new Claim(ClaimTypes.Role, claims.role.ToString()),
                new Claim("companyId", claims.companyId.ToString()),
            };

        }
    }

    public class AuthOptions
    {
        public string ISSUER { get; set; }
        public string AUDIENCE { get; set; }
        public string KEY { get; set; }

        public SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
    }

    public class jwtService : IJwt
    {
        private readonly AuthOptions _authOptions;

        public jwtService(IOptions<AuthOptions> authOptions)
        {
            _authOptions = authOptions.Value;
        }

        public async Task<string> GenerateAccessTokenAsync(Claims claims)
        {
            var jwt = new JwtSecurityToken(
            issuer: _authOptions.ISSUER,
            audience: _authOptions.AUDIENCE,

            claims: ClaimsService.GetClaims(claims),

            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
            signingCredentials: new SigningCredentials(_authOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public string GenerateRefreshTokenAsync()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public void ValidateToken(string token)
        {

        }
    }

}
