using Auth.service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class authController : ControllerBase
    {
        private readonly SignIn _signInService;
        private readonly SignUp _signUpService;
        public authController(SignIn signInService, SignUp signUpService)
        {
            _signInService = signInService;
        }

        public class SignInDto
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class SignUpDto : SignInDto
        {
            public string Name { get; set; }
        }

        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn(SignInDto signin)
        {
            var response = await _signInService.SignInAsync(signin.Email, signin.Password);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("sign-up")]
        public async Task<IActionResult> SignUp(SignUpDto signup)
        {
            var response = await _signUpService.SignUpAsync(signup.Email, signup.Password, signup.Name);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
