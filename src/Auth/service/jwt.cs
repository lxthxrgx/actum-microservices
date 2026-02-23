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
        public string GenerateAccessTokenAsync(Claims claims);
        public string GenerateRefreshTokenAsync();
        public ClaimsPrincipal ValidateToken(string token);
    }

    public interface IClaims
    {
        Guid userId { get; set; }
        Guid? Company { get; set; }
        CompanyUserRole role { get; set; }
    }

    public class Claims : IClaims
    {
        public Guid userId { get; set; }
        public Guid? Company { get; set; }
        public CompanyUserRole role { get; set; }
    }

    public static class ClaimsService
    {
        public static List<Claim> GetClaims(Claims claims)
        {
            var claimsList = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, claims.userId.ToString()),
                new Claim(ClaimTypes.Role, claims.role.ToString())
            };
            if (claims.Company.HasValue)
            {
                claimsList.Add(new Claim("Company", claims.Company.Value.ToString()));
            }
            return claimsList;
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

        public string GenerateAccessTokenAsync(Claims claims)
        {
            try
            {
                var jwt = new JwtSecurityToken(
                   issuer: _authOptions.ISSUER,
                   audience: _authOptions.AUDIENCE,

                   claims: ClaimsService.GetClaims(claims),

                   expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
                   signingCredentials: new SigningCredentials(_authOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

                var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

                return accessToken;
            }
            catch (Exception ex) 
            {
                return null;
            }
           
        }

        public string GenerateRefreshTokenAsync()
        {
            try
            {
                var randomNumber = new byte[64];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomNumber);
                var refreshToken = Convert.ToBase64String(randomNumber);
                return refreshToken;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_authOptions.KEY);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _authOptions.ISSUER,
                    ValidateAudience = true,
                    ValidAudience = _authOptions.AUDIENCE,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }

}
