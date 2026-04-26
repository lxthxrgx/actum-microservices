using Auth.service;
using Microsoft.AspNetCore.Mvc;
using SharedLibraries.Factory;
using SharedLibraries.model;

namespace Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class authController : ControllerBase
    {
        private readonly SignIn      _signInService;
        private readonly SignUp      _signUpService;
        private readonly jwtService  _jwtService;

        public authController(SignIn signInService, SignUp signUpService, jwtService jwtService)
        {
            _signInService = signInService;
            _signUpService = signUpService;
            _jwtService    = jwtService;
        }

        public class SignInDto
        {
            public string Email    { get; set; }
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
                return BadRequest(response);

            if (response is not IOk<Claims> ok || ok.Data == null || !ok.Data.Any())
                return BadRequest("Claims data is missing.");

            var claims = ok.Data.First();

            var accessToken  = _jwtService.GenerateAccessTokenAsync(claims);
            var refreshToken = _jwtService.GenerateRefreshTokenAsync();

            // Возвращаем токены в JSON — Gateway перехватит и установит HttpOnly куки
            return Ok(new
            {
                message      = response.Message,
                accessToken  = accessToken,
                refreshToken = refreshToken
            });
        }

        [HttpPost("sign-up")]
        public async Task<IActionResult> SignUp(SignUpDto signup)
        {
            var response = await _signUpService.SignUpAsync(signup.Email, signup.Password, signup.Name);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
