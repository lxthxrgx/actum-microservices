using Auth.service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers
{
    [Route("api/auth/[controller]")]
    [ApiController]
    public class SignInController : ControllerBase
    {
        private readonly SignIn _signInService;

        public SignInController(SignIn signInService)
        {
            _signInService = signInService;
        }

        public class SignInDto
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(SignInDto signin)
        {
            var response = await _signInService.SignInAsync(signin.Email, signin.Password);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
