using Auth.service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers
{
    [Route("api/auth/[controller]")]
    [ApiController]
    public class SignUpController : ControllerBase
    {
        private readonly SignUp _signUpService;

        public SignUpController(SignUp signUpService)
        {
            _signUpService = signUpService;
        }

        public class SignUpDto
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string Name { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(SignUpDto signup)
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
