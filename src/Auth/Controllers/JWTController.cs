using Auth.service;
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

        [HttpGet]
        public async Task<IActionResult> GetToken()
        {
            var claims = new Claims
            {
                userId = Guid.NewGuid(),
                companyId = Guid.NewGuid(),
                role = CompanyUserRole.Admin
            };
            var token = await _jwtService.GenerateAccessTokenAsync(claims);
            return Ok(token);
        }
    }
}
