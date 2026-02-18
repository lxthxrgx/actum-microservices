using Auth.service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibraries.model;

namespace Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JWTController : ControllerBase
    {
        private readonly jwtService _jwtService;

        public JWTController(jwtService jwtService)
        {
            _jwtService = jwtService;
        }

        public class Clam
        {
            public Guid userId { get; set; }
            public Guid companyId { get; set; }
            public CompanyUserRole role { get; set; }
        }

        [HttpPost("token")]
        public async Task<IActionResult> GetToken([FromBody]Clam clam)
        {
            var claims = new Claims
            {
                userId = clam.userId,
                companyId = clam.companyId,
                role = clam.role
            };
            var token = _jwtService.GenerateAccessTokenAsync(claims);
            return Ok(token);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Protect()
        {
            return Ok("Token is valid.");
        }
    }
}
